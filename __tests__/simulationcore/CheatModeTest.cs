using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class CheatModeTest
{
	[TestCase]
	public void EnablingCheatModeGrantsResourcesAndCompletesCurrentWorkForOnePlayer()
	{
		var context = new SimulationContext("cheat-test");
		var player = new PlayerState("player", 5000);
		var opponent = new PlayerState("opponent", 5000);
		context.AddPlayer(player);
		context.AddPlayer(opponent);
		var builder = new UnitState("builder", player.PlayerId, new Vector2(800, 0), 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.COMMAND_CENTER, Vector2.Zero))).IsTrue();
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.BARRACKS, new Vector2(400, 0)))).IsTrue();
		var barracks = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.BARRACKS);
		AssertThat(context.Push(new BuildStructureMessage(BuildingType.POWER_PLANT, player.PlayerId, 0, builder.EntityId, new Vector2(1000, 0)))).IsTrue();
		var constructionSite = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.CONSTRUCTION_SITE);
		AssertThat(context.Push(new StartResearchMessage(player.PlayerId, 0,
			player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.COMMAND_CENTER).EntityId,
			"light_tanks"))).IsTrue();
		AssertThat(context.Push(new TrainUnitsMessage(UnitType.BASIC_INFANTRY, player.PlayerId, 0, barracks.EntityId, 3))).IsTrue();

		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, true))).IsTrue();

		AssertThat(player.CheatModeEnabled).IsTrue();
		AssertThat(player.Virelium).IsEqual(1_000_000);
		AssertThat(constructionSite.Type).IsEqual(BuildingType.POWER_PLANT);
		AssertThat(builder.HasConstructionOrder).IsFalse();
		AssertThat(builder.HasMoveOrder).IsFalse();
		AssertThat(player.GetResearchLevel("light_tanks")).IsEqual(1);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.EnergyConsumed).IsEqual(0);
		AssertThat(barracks.ProductionQueue).IsEmpty();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count(unit => unit.Type == UnitType.BASIC_INFANTRY)).IsEqual(1);
		AssertThat(opponent.CheatModeEnabled).IsFalse();
		AssertThat(opponent.Virelium).IsEqual(5000);

		context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count(unit => unit.Type == UnitType.BASIC_INFANTRY)).IsEqual(1);
		context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count(unit => unit.Type == UnitType.BASIC_INFANTRY)).IsEqual(2);
	}

	[TestCase]
	public void EnabledCheatModeMakesNewPaidWorkInstantDespitePowerDeficit()
	{
		var context = new SimulationContext("cheat-test");
		var player = new PlayerState("player", 2000);
		context.AddPlayer(player);
		var builder = new UnitState("builder", player.PlayerId, new Vector2(800, 0), 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.COMMAND_CENTER, Vector2.Zero))).IsTrue();
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.WAR_FACTORY, new Vector2(400, 0)))).IsTrue();
		var commandCenter = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.COMMAND_CENTER);
		var warFactory = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.WAR_FACTORY);
		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, true))).IsTrue();

		var balance = player.Virelium;
		var buildCost = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT).Single(ability => ability.Id == "spawn_barracks").Cost;
		AssertThat(context.Push(new UseAbilityMessage(player.PlayerId, 0, new[] { builder.EntityId }, "spawn_barracks",
			p_target_position: new Vector2(1000, 0)))).IsTrue();
		AssertThat(player.Entities.Values.OfType<BuildingState>().Single(building => building.CurrentPosition == new Vector2(1000, 0)).Type).IsEqual(BuildingType.BARRACKS);
		AssertThat(player.Virelium).IsEqual(balance - buildCost);

		balance = player.Virelium;
		var researchCost = ResearchCatalog.All.Single(technology => technology.Id == "light_tanks").GetMaterialCost(1);
		AssertThat(context.Push(new StartResearchMessage(player.PlayerId, 0, commandCenter.EntityId, "light_tanks"))).IsTrue();
		AssertThat(player.GetResearchLevel("light_tanks")).IsEqual(1);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.Virelium).IsEqual(balance - researchCost);

		balance = player.Virelium;
		var tankCost = AbilityCatalog.ForBuilding(BuildingType.WAR_FACTORY).Single(ability => ability.Id == "spawn_light_tank").Cost;
		AssertThat(context.Push(new UseAbilityMessage(player.PlayerId, 0, new[] { warFactory.EntityId }, "spawn_light_tank"))).IsTrue();
		AssertThat(warFactory.ProductionQueue).IsEmpty();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count(unit => unit.Type == UnitType.LIGHT_TANK)).IsEqual(1);
		AssertThat(player.Virelium).IsEqual(balance - tankCost);
	}

	[TestCase]
	public void DisablingCheatModeRestoresTimersWithoutGrantingResourcesAgain()
	{
		var context = new SimulationContext("cheat-test");
		var player = new PlayerState("player");
		context.AddPlayer(player);
		var builder = new UnitState("builder", player.PlayerId, new Vector2(800, 0), 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.COMMAND_CENTER, Vector2.Zero))).IsTrue();
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.BARRACKS, new Vector2(400, 0)))).IsTrue();
		var commandCenter = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.COMMAND_CENTER);
		var barracks = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.BARRACKS);

		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, true))).IsTrue();
		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, false))).IsTrue();
		player.RemoveMaterials(player.Virelium - 123);
		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, true))).IsTrue();
		AssertThat(player.Virelium).IsEqual(123);
		AssertThat(context.Push(new SetCheatModeMessage(player.PlayerId, 0, false))).IsTrue();
		player.AddMaterials(5000);

		AssertThat(context.Push(new BuildStructureMessage(BuildingType.POWER_PLANT, player.PlayerId, 0, builder.EntityId, new Vector2(1000, 0)))).IsTrue();
		AssertThat(player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.CONSTRUCTION_SITE).BuildProgression).IsEqual(0);
		AssertThat(context.Push(new StartResearchMessage(player.PlayerId, 0, commandCenter.EntityId, "light_tanks"))).IsTrue();
		AssertThat(player.ResearchProjects.ContainsKey("light_tanks")).IsTrue();
		AssertThat(context.Push(new TrainUnitsMessage(UnitType.BASIC_INFANTRY, player.PlayerId, 0, barracks.EntityId))).IsTrue();
		AssertThat(barracks.ProductionQueue.Count).IsEqual(1);
	}
}
