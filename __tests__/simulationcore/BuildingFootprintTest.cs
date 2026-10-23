using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class BuildingFootprintTest
{
	private static readonly (BuildingType Type, string Scene)[] Buildings =
	{
		(BuildingType.BARRACKS, "Barracks"),
		(BuildingType.COMMAND_CENTER, "CommandCenter"),
		(BuildingType.POWER_PLANT, "PowerPlant"),
		(BuildingType.RESOURCE_GATHERER, "Resource_Gatherer"),
		(BuildingType.WAR_FACTORY, "WarFactory"),
		(BuildingType.RESOURCE_SPAWNER, "ResourceSpawner")
	};

	[TestCase]
	public void CatalogFootprints_MatchAuthoredSceneCollisions()
	{
		foreach (var (type, sceneName) in Buildings)
		{
			var scene = GD.Load<PackedScene>($"res://scenes/buildings/{sceneName}.tscn").Instantiate<Node2D>();
			try
			{
				var collision = scene.GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
				var shape = (RectangleShape2D)collision.Shape;
				var localTransform = scene.Transform.AffineInverse() * collision.GlobalTransform;
				var topLeft = localTransform * (-shape.Size / 2f);
				var bottomRight = localTransform * (shape.Size / 2f);
				AssertThat(BuildingCatalog.GetFootprintRect(type, Vector2.Zero))
					.IsEqual(new Rect2(topLeft, bottomRight - topLeft));
			}
			finally
			{
				scene.Free();
			}
		}
	}

	[TestCase]
	public void ConstructionFootprints_MatchFinalBuildings_AndKeepShapesIndependent()
	{
		var packedScene = GD.Load<PackedScene>("res://scenes/buildings/ConstructionSite.tscn");
		var site = packedScene.Instantiate<ConstructionSite>();
		var otherSite = packedScene.Instantiate<ConstructionSite>();
		try
		{
			otherSite.ApplyFootprint(BuildingType.POWER_PLANT);
			foreach (var (type, _) in Buildings)
			{
				site.ApplyFootprint(type);
				var expected = BuildingCatalog.GetFootprintRect(type, Vector2.Zero);
				var body = site.GetNode<ColorRect>("BodyRender");
				var collision = site.GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
				AssertThat(new Rect2(body.Position, body.Size)).IsEqual(expected);
				AssertThat(collision.Position).IsEqual(expected.GetCenter());
				AssertThat(((RectangleShape2D)collision.Shape).Size).IsEqual(expected.Size);

				var state = new BuildingState("site", "player", new Vector2(350, 200), type, "");
				var reservedFootprint = BuildingCatalog.GetFootprintRect(state);
				state.AdvanceConstruction(100);
				AssertThat(BuildingCatalog.GetFootprintRect(state)).IsEqual(reservedFootprint);
				AssertThat(state.RallyPoint).IsEqual(reservedFootprint.End + new Vector2(20, 20));
			}
			AssertThat(((RectangleShape2D)otherSite.GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Shape).Size)
				.IsEqual(BuildingCatalog.GetFoodprintSize(BuildingType.POWER_PLANT));
		}
		finally
		{
			site.Free();
			otherSite.Free();
		}
	}

	[TestCase]
	public void Placement_RejectsNeutralResources_WithoutSpendingOrAssigningOrders()
	{
		var context = new SimulationContext("match");
		var player = new PlayerState("player", 2000);
		var builder = new UnitState("builder", "player", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);
		var position = new Vector2(400, 300);
		((MatchState)context.get()).AddResource(new ResourceState("resource", ResourceType.VIRELIUM, "spawner", position, 2500));

		AssertThat(context.Push(new UseAbilityMessage("player", 0, new[] { "builder" },
			"spawn_power_plant", p_target_position: position))).IsFalse();
		AssertThat(context.Push(new BuildStructureMessage(BuildingType.POWER_PLANT, "player", 0, "builder", position))).IsFalse();
		AssertThat(player.Virelium).IsEqual(2000);
		AssertThat(player.Entities.Values.OfType<BuildingState>().Any()).IsFalse();
		AssertThat(builder.HasConstructionOrder).IsFalse();
	}

	[TestCase]
	public void Placement_ResourceEdgesAndDepletion_AllowBuilding()
	{
		var context = new SimulationContext("match");
		var player = new PlayerState("player");
		player.AddEntity(new UnitState("builder", "player", new Vector2(500, 500), 100f, UnitType.CONSTRUCTION_UNIT));
		context.AddPlayer(player);
		var resource = new ResourceState("resource", ResourceType.VIRELIUM, "spawner", Vector2.Zero, 2500);
		((MatchState)context.get()).AddResource(resource);

		// The power plant's left edge is 92 units left of its origin; the resource ends at x = 16.
		AssertThat(BuildingCatalog.IsPlacementValid(context.get(), BuildingType.POWER_PLANT, new Vector2(107, 0))).IsFalse();
		AssertThat(BuildingCatalog.IsPlacementValid(context.get(), BuildingType.POWER_PLANT, new Vector2(108, 0))).IsTrue();
		resource.Extract(resource.MaxAmount);
		AssertThat(context.Push(new BuildStructureMessage(BuildingType.POWER_PLANT, "player", 0, "builder", Vector2.Zero))).IsTrue();
	}

	[TestCase]
	public void Placement_UsesOffsets_AndAllowsTouchingEdges()
	{
		// Command Center spans x = -90..230; Barracks spans x = -119..120.
		AssertThat(BuildingCatalog.FootprintsOverlap(
			BuildingType.COMMAND_CENTER, Vector2.Zero,
			BuildingType.BARRACKS, new Vector2(348, 0))).IsTrue();
		AssertThat(BuildingCatalog.FootprintsOverlap(
			BuildingType.COMMAND_CENTER, Vector2.Zero,
			BuildingType.BARRACKS, new Vector2(349, 0))).IsFalse();
		// Barracks spans y = -51..80; Power Plant spans y = -45..41.
		AssertThat(BuildingCatalog.FootprintsOverlap(
			BuildingType.BARRACKS, Vector2.Zero,
			BuildingType.POWER_PLANT, new Vector2(0, 124))).IsTrue();
		AssertThat(BuildingCatalog.FootprintsOverlap(
			BuildingType.BARRACKS, Vector2.Zero,
			BuildingType.POWER_PLANT, new Vector2(0, 125))).IsFalse();
	}

	[TestCase]
	public void Construction_CompletesAtTheOffsetFootprint_ForEveryBuilding()
	{
		foreach (var (type, _) in Buildings)
		{
			var context = new SimulationContext("match");
			var player = new PlayerState("player");
			var position = new Vector2(400, 300);
			var footprint = BuildingCatalog.GetFootprintRect(type, position);
			var builder = new UnitState("builder", "player", footprint.End + new Vector2(50, 50), 100f, UnitType.CONSTRUCTION_UNIT);
			player.AddEntity(builder);
			context.AddPlayer(player);
			AssertThat(context.Push(new BuildStructureMessage(type, "player", 0, "builder", position))).IsTrue();
			var site = player.Entities.Values.OfType<BuildingState>().Single();
			for (var i = 0; i < 80 && site.Type == BuildingType.CONSTRUCTION_SITE; i++)
			{
				context.AdvanceTick();
				AssertThat(footprint.HasPoint(builder.CurrentPosition)).IsFalse();
			}
			AssertThat(site.Type).IsEqual(type);
			AssertThat(builder.HasConstructionOrder).IsFalse();
		}
	}

	[TestCase]
	public void Move_IntoLargeBuilding_ResolvesDestinationOutsideFootprint()
	{
		var context = new SimulationContext("match");
		var player = new PlayerState("player");
		var building = new BuildingState("building", "player", Vector2.Zero, BuildingType.COMMAND_CENTER, "");
		building.AdvanceConstruction(100);
		var unit = new UnitState("unit", "player", new Vector2(500, 100), 100f);
		player.AddEntity(building);
		player.AddEntity(unit);
		context.AddPlayer(player);
		// This point is far from the root origin, but inside the scene collision.
		AssertThat(context.Push(new MoveUnitsMessage("player", 0, new[] { "unit" }, new Vector2(200, 100)))).IsTrue();
		AssertThat(BuildingCatalog.GetFootprintRect(building).HasPoint(unit.TargetPosition)).IsFalse();
		for (var i = 0; i < 40 && unit.HasMoveOrder; i++)
		{
			context.AdvanceTick();
			AssertThat(BuildingCatalog.GetFootprintRect(building).HasPoint(unit.CurrentPosition)).IsFalse();
		}
		AssertThat(unit.HasMoveOrder).IsFalse();
	}

	[TestCase]
	public void Attack_LargeBuilding_StopsOutsideFootprint_AndDealsDamage()
	{
		var context = new SimulationContext("match");
		var player = new PlayerState("player");
		var enemy = new PlayerState("enemy");
		var attacker = new UnitState("attacker", "player", new Vector2(500, 62), 100f, UnitType.RPG_TROOPER);
		var building = new BuildingState("building", "enemy", Vector2.Zero, BuildingType.COMMAND_CENTER, "");
		building.AdvanceConstruction(100);
		player.AddEntity(attacker);
		enemy.AddEntity(building);
		context.AddPlayer(player);
		context.AddPlayer(enemy);
		AssertThat(context.Push(new AttackTargetMessage("player", 0, new[] { "attacker" }, "building"))).IsTrue();
		for (var i = 0; i < 30; i++)
		{
			context.AdvanceTick();
			AssertThat(BuildingCatalog.GetFootprintRect(building).HasPoint(attacker.CurrentPosition)).IsFalse();
		}
		AssertThat(building.Health < building.MaxHealth).IsTrue();
		AssertThat(attacker.HasMoveOrder).IsFalse();
	}
}
