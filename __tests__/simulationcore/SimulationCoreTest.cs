using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class SimulationCoreTest
{
	[TestCase]
	public void Push_ReturnsFalse_WhenMessageIsNotACommand()
	{
		var context = new SimulationContext("match-1");
		var msg = new PingMessage(p_client_time_unix_ms: 123);

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_ReturnsFalse_WhenCommandValidationFails()
	{
		var context = new SimulationContext("match-1");
		var msg = new MoveUnitsMessage(
			p_player_id: "player-1",
			p_issued_at_tick: -1,
			p_unit_ids: new[] { "unit-1" },
			p_destination: new Vector2(1, 2)
		);

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_ReturnsFalse_WhenCommandHasNoRegisteredHandler()
	{
		var context = new SimulationContext("match-1");
		var msg = new StopUnitsMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "unit-1" }
		);

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_MoveUnitsMessage_ReturnsFalse_WhenPlayerDoesNotExist()
	{
		var context = new SimulationContext("match-1");

		var msg = new MoveUnitsMessage(
			p_player_id: "1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] {"unit_1"},
			p_destination: new Vector2(1, 2)
		);

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_MoveUnitsMessage_ReturnsFalse_WhenEntityDoesNotExist()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		var msg = CreateMoveMessage("player-1", "missing-unit", new Vector2(1, 2));

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_MoveUnitsMessage_ReturnsFalse_WhenEntityIsNotAUnit()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var building = new BuildingState("building-1", "player-1", Vector2.Zero, BuildingType.BARRACKS, "construction-1");
		player.AddEntity(building);
		context.AddPlayer(player);

		var msg = CreateMoveMessage("player-1", "building-1", new Vector2(1, 2));

		AssertThat(context.Push(msg)).IsFalse();
	}

	[TestCase]
	public void Push_MoveUnitsMessage_AssignsMoveOrderToUnit()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var unit = new UnitState("unit-1", "player-1", Vector2.Zero, 10f);
		player.AddEntity(unit);
		context.AddPlayer(player);

		var destination = new Vector2(10, 20);
		var msg = CreateMoveMessage("player-1", "unit-1", destination);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(unit.HasMoveOrder).IsTrue();
		AssertThat(unit.TargetPosition).IsEqual(destination);
	}

	[TestCase]
	public void Push_MoveUnitsMessage_AssignsMoveOrderToEverySelectedUnit()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var unitA = new UnitState("unit-a", "player-1", Vector2.Zero, 10f);
		var unitB = new UnitState("unit-b", "player-1", Vector2.Zero, 10f);
		player.AddEntity(unitA);
		player.AddEntity(unitB);
		context.AddPlayer(player);

		var destination = new Vector2(30, 40);
		var msg = new MoveUnitsMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "unit-a", "unit-b" },
			p_destination: destination
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(unitA.HasMoveOrder).IsTrue();
		AssertThat(unitA.TargetPosition).IsEqual(destination + new Vector2(-25f, 0f));
		AssertThat(unitB.HasMoveOrder).IsTrue();
		AssertThat(unitB.TargetPosition).IsEqual(destination + new Vector2(25f, 0f));
	}

	[TestCase]
	public void AdvanceTick_MoveUnits_RoutesAroundBuildingFootprints_FromLeftToRight()
	{
		AssertMoveRoutesAroundBuilding(Vector2.Zero, new Vector2(200, 0), movedOffAxis: point => System.Math.Abs(point.Y) > 1f);
	}

	[TestCase]
	public void AdvanceTick_MoveUnits_RoutesAroundBuildingFootprints_FromRightToLeft()
	{
		AssertMoveRoutesAroundBuilding(new Vector2(200, 0), Vector2.Zero, movedOffAxis: point => System.Math.Abs(point.Y) > 1f);
	}

	[TestCase]
	public void AdvanceTick_MoveUnits_RoutesAroundBuildingFootprints_FromTopToBottom()
	{
		AssertMoveRoutesAroundBuilding(new Vector2(100, -100), new Vector2(100, 100), movedOffAxis: point => System.Math.Abs(point.X - 100f) > 1f);
	}

	[TestCase]
	public void AdvanceTick_MoveUnits_RoutesAroundBuildingFootprints_FromBottomToTop()
	{
		AssertMoveRoutesAroundBuilding(new Vector2(100, 100), new Vector2(100, -100), movedOffAxis: point => System.Math.Abs(point.X - 100f) > 1f);
	}

	[TestCase]
	public void Push_DebugSpawnUnit_AssignsDistinctMoveTargets_WhenSpawnPositionIsOccupied()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		var position = new Vector2(10, 20);

		AssertThat(context.Push(CreateSpawnMessage("player-1", position))).IsTrue();
		AssertThat(context.Push(CreateSpawnMessage("player-1", position))).IsTrue();
		AssertThat(context.Push(CreateSpawnMessage("player-1", position))).IsTrue();

		var units = player.Entities.Values.OfType<UnitState>().ToArray();
		AssertThat(units.Length).IsEqual(3);
		AssertThat(units.All(unit => unit.CurrentPosition == position)).IsTrue();
		AssertThat(units.Count(unit => !unit.HasMoveOrder)).IsEqual(1);
		AssertThat(units.Any(unit => unit.TargetPosition == position + new Vector2(50f, 0f))).IsTrue();
		AssertThat(units.Any(unit => unit.TargetPosition == position + new Vector2(-50f, 0f))).IsTrue();
	}

	[TestCase]
	public void Push_DebugSpawnUnit_ResourceCollector_CreatesResourceCollectorState()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnUnitsMessage(
			"player-1",
			0,
			UnitType.RESOURCE_COLLECTOR,
			Vector2.Zero,
			Vector2.Zero,
			UnitCatalog.GetMovementSpeed(UnitType.RESOURCE_COLLECTOR)
		))).IsTrue();

		AssertThat(player.Entities.Values.Single()).IsInstanceOf<ResourceCollectorState>();
	}

	[TestCase]
	public void Push_DebugSpawnBuilding_CreatesCompletedBuildingAtPosition()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		var position = new Vector2(30, 40);
		var msg = new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.BARRACKS,
			position
		);

		AssertThat(context.Push(msg)).IsTrue();

		var building = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(building.Type).IsEqual(BuildingType.BARRACKS);
		AssertThat(building.CurrentPosition).IsEqual(position);
		AssertThat(building.BuildProgression).IsEqual(100);
		AssertThat(building.Health).IsEqual(BuildingCatalog.GetMaxHealth(BuildingType.BARRACKS));
	}

	[TestCase]
	public void Push_DebugSpawnBuilding_CreatesConstructionSite_WhenSpawnCompletedIsFalse()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		var msg = new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.BARRACKS,
			Vector2.Zero,
			p_spawn_completed: false
		);

		AssertThat(context.Push(msg)).IsTrue();

		var building = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(building.Type).IsEqual(BuildingType.CONSTRUCTION_SITE);
		AssertThat(building.pendingBuilding).IsEqual(BuildingType.BARRACKS);
		AssertThat(building.BuildProgression).IsEqual(0);
	}

	[TestCase]
	public void AbilityCatalog_ForBuilding_AddsSellToCompletedBuildingsOnly()
	{
		var completedBuildingTypes = System.Enum.GetValues<BuildingType>()
			.Where(type => type != BuildingType.CONSTRUCTION_SITE);

		foreach (var buildingType in completedBuildingTypes)
		{
			AssertThat(AbilityCatalog.ForBuilding(buildingType).Any(ability => ability.Id == "sell_building")).IsTrue();
		}

		AssertThat(AbilityCatalog.ForBuilding(BuildingType.CONSTRUCTION_SITE)
			.Any(ability => ability.Id == "sell_building")).IsFalse();
	}

	[TestCase]
	public void Push_DebugSpawnBuilding_ResourceSpawner_CreatesTrackedResourcesImmediately()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		var msg = new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.RESOURCE_SPAWNER,
			Vector2.Zero
		);

		AssertThat(context.Push(msg)).IsTrue();

		var spawner = player.Entities.Values.OfType<BuildingState>().Single();
		var resourceCount = ResourceCatalog.GetSpawnerResourceCount(BuildingType.RESOURCE_SPAWNER);
		AssertThat(spawner.Type).IsEqual(BuildingType.RESOURCE_SPAWNER);
		AssertThat(spawner.HasSpawnedResources).IsTrue();
		AssertThat(spawner.ResourceEntityIds.Count).IsEqual(resourceCount);
		AssertThat(context.get().Resources.Count).IsEqual(resourceCount);
		AssertThat(context.get().Resources.Values.All(resource => resource.SpawnerEntityId == spawner.EntityId)).IsTrue();
	}

	[TestCase]
	public void AdvanceTick_BasicInfantryWaitsForCooldown_AfterFiring_WhenEnemyIsInRange()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var enemyPlayer = new PlayerState("player-2");
		var attacker = new UnitState("attacker-1", "player-1", Vector2.Zero, 10f, UnitType.BASIC_INFANTRY);
		var target = new UnitState("target-1", "player-2", new Vector2(50, 0), 10f, UnitType.BASIC_INFANTRY);
		player.AddEntity(attacker);
		enemyPlayer.AddEntity(target);
		context.AddPlayer(player);
		context.AddPlayer(enemyPlayer);

		var msg = new AttackTargetMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "attacker-1" },
			p_target_id: "target-1"
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(target.Health).IsEqual(target.MaxHealth);

		var attackDamage = ExpectedDamage(attacker, target);

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth);

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth - attackDamage);
		AssertThat(attacker.HasAttackOrder).IsTrue();
		AssertThat(attacker.AttackCooldownRemaining).IsEqual(UnitCatalog.GetAttackCooldownTime(UnitType.BASIC_INFANTRY));

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth - attackDamage);

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth - attackDamage);

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth - attackDamage * 2);
	}

	[TestCase]
	public void AdvanceTick_AppliesWeaponEffectiveness_ToAttackDamage()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var enemyPlayer = new PlayerState("player-2");
		var attacker = new UnitState("attacker-1", "player-1", Vector2.Zero, 10f, UnitType.RPG_TROOPER);
		var target = new UnitState("target-1", "player-2", new Vector2(50, 0), 10f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(attacker);
		enemyPlayer.AddEntity(target);
		context.AddPlayer(player);
		context.AddPlayer(enemyPlayer);

		var msg = new AttackTargetMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "attacker-1" },
			p_target_id: "target-1"
		);

		AssertThat(context.Push(msg)).IsTrue();

		for (var i = 0; i < 5; i++)
			context.AdvanceTick();

		AssertThat(target.Health).IsEqual(target.MaxHealth - ExpectedDamage(attacker, target));
	}

	[TestCase]
	public void AdvanceTick_BasicInfantryMovesTowardAttackTarget_WhenEnemyIsOutOfRange()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var enemyPlayer = new PlayerState("player-2");
		var attacker = new UnitState("attacker-1", "player-1", Vector2.Zero, 100f, UnitType.BASIC_INFANTRY);
		var target = new UnitState("target-1", "player-2", new Vector2(200, 0), 10f, UnitType.BASIC_INFANTRY);
		player.AddEntity(attacker);
		enemyPlayer.AddEntity(target);
		context.AddPlayer(player);
		context.AddPlayer(enemyPlayer);

		var msg = new AttackTargetMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "attacker-1" },
			p_target_id: "target-1"
		);

		AssertThat(context.Push(msg)).IsTrue();

		context.AdvanceTick();

		AssertThat(target.Health).IsEqual(target.MaxHealth);
		AssertThat(attacker.HasAttackOrder).IsTrue();
		AssertThat(attacker.HasMoveOrder).IsTrue();
		AssertThat(attacker.CurrentPosition).IsEqual(new Vector2(20, 0));
		AssertThat(attacker.AttackWindupProgress).IsEqual(0f);
	}

	[TestCase]
	public void AdvanceTick_MultipleBasicInfantryMoveTowardAttackTargetInFormation_WhenEnemyIsOutOfRange()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var enemyPlayer = new PlayerState("player-2");
		var attackerA = new UnitState("attacker-a", "player-1", Vector2.Zero, 100f, UnitType.BASIC_INFANTRY);
		var attackerB = new UnitState("attacker-b", "player-1", new Vector2(0, 50), 100f, UnitType.BASIC_INFANTRY);
		var target = new UnitState("target-1", "player-2", new Vector2(300, 0), 10f, UnitType.BASIC_INFANTRY);
		player.AddEntity(attackerA);
		player.AddEntity(attackerB);
		enemyPlayer.AddEntity(target);
		context.AddPlayer(player);
		context.AddPlayer(enemyPlayer);

		var msg = new AttackTargetMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "attacker-a", "attacker-b" },
			p_target_id: "target-1"
		);

		AssertThat(context.Push(msg)).IsTrue();

		context.AdvanceTick();

		AssertThat(attackerA.HasAttackOrder).IsTrue();
		AssertThat(attackerB.HasAttackOrder).IsTrue();
		AssertThat(attackerA.HasMoveOrder).IsTrue();
		AssertThat(attackerB.HasMoveOrder).IsTrue();
		AssertThat(attackerA.TargetPosition).IsEqual(target.CurrentPosition + new Vector2(-25f, 0f));
		AssertThat(attackerB.TargetPosition).IsEqual(target.CurrentPosition + new Vector2(25f, 0f));
	}

	[TestCase]
	public void AdvanceTick_BasicInfantryKeepsFollowingAttackTarget_WhenTargetMovesOutOfRange()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var enemyPlayer = new PlayerState("player-2");
		var attacker = new UnitState("attacker-1", "player-1", Vector2.Zero, 100f, UnitType.BASIC_INFANTRY);
		var target = new UnitState("target-1", "player-2", new Vector2(50, 0), 1000f, UnitType.BASIC_INFANTRY);
		player.AddEntity(attacker);
		enemyPlayer.AddEntity(target);
		context.AddPlayer(player);
		context.AddPlayer(enemyPlayer);

		var msg = new AttackTargetMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_unit_ids: new[] { "attacker-1" },
			p_target_id: "target-1"
		);

		AssertThat(context.Push(msg)).IsTrue();
		target.SetMoveOrder(new Vector2(200, 0));

		context.AdvanceTick();
		context.AdvanceTick();

		AssertThat(attacker.HasAttackOrder).IsTrue();
		AssertThat(attacker.HasMoveOrder).IsTrue();
		AssertThat(target.Health).IsEqual(target.MaxHealth);
	}

	[TestCase]
	public void Push_BuildStructure_AssignsConstructionUnitToCreatedSite()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		var msg = new BuildStructureMessage(
			BuildingType.BARRACKS,
			"player-1",
			0,
			"builder-1",
			Vector2.Zero
		);

		AssertThat(context.Push(msg)).IsTrue();
		var site = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(site.ConstructionUnitId).IsEqual("builder-1");
		AssertThat(builder.HasConstructionOrder).IsTrue();
		AssertThat(builder.ConstructionTargetId).IsEqual(site.EntityId);
		AssertThat(builder.HasMoveOrder).IsTrue();
	}

	[TestCase]
	public void Push_MoveUnits_ClearsConstructionOrderAndStopsProgress()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(context.Push(new BuildStructureMessage(
			BuildingType.BARRACKS,
			"player-1",
			0,
			"builder-1",
			Vector2.Zero
		))).IsTrue();
		var site = player.Entities.Values.OfType<BuildingState>().Single();

		AssertThat(context.Push(CreateMoveMessage("player-1", "builder-1", new Vector2(200, 0)))).IsTrue();
		context.AdvanceTick();

		AssertThat(builder.HasConstructionOrder).IsFalse();
		AssertThat(site.BuildProgression).IsEqual(0);
	}

	[TestCase]
	public void Push_RepairTarget_AssignsSelectedConstructionUnitToConstructionSite()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var firstBuilder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		var secondBuilder = new UnitState("builder-2", "player-1", new Vector2(100, 0), 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(firstBuilder);
		player.AddEntity(secondBuilder);
		context.AddPlayer(player);

		AssertThat(context.Push(new BuildStructureMessage(
			BuildingType.BARRACKS,
			"player-1",
			0,
			"builder-1",
			Vector2.Zero
		))).IsTrue();
		var site = player.Entities.Values.OfType<BuildingState>().Single();

		var assignMessage = new RepairTargetMessage(
			"player-1",
			0,
			new[] { "builder-2" },
			site.EntityId
		);

		AssertThat(context.Push(assignMessage)).IsTrue();
		AssertThat(site.ConstructionUnitId).IsEqual("builder-2");
		AssertThat(firstBuilder.HasConstructionOrder).IsFalse();
		AssertThat(secondBuilder.HasConstructionOrder).IsTrue();
		AssertThat(secondBuilder.ConstructionTargetId).IsEqual(site.EntityId);
	}

	[TestCase]
	public void AdvanceTick_CompletedResourceSpawner_CreatesTrackedResourceStates()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(context.Push(new BuildStructureMessage(
			BuildingType.RESOURCE_SPAWNER,
			"player-1",
			0,
			"builder-1",
			Vector2.Zero
		))).IsTrue();

		var spawner = player.Entities.Values.OfType<BuildingState>().Single();
		spawner.AdvanceConstruction(100);

		context.AdvanceTick();

		var resourceCount = ResourceCatalog.GetSpawnerResourceCount(BuildingType.RESOURCE_SPAWNER);
		AssertThat(spawner.Type).IsEqual(BuildingType.RESOURCE_SPAWNER);
		AssertThat(spawner.HasSpawnedResources).IsTrue();
		AssertThat(spawner.ResourceEntityIds.Count).IsEqual(resourceCount);
		AssertThat(context.get().Resources.Count).IsEqual(resourceCount);
		AssertThat(context.get().Resources.Values.All(resource =>
			resource.SpawnerEntityId == spawner.EntityId &&
			resource.ResourceType == ResourceType.VIRELIUM &&
			resource.CurrentAmount == ResourceCatalog.GetMaxAmount(ResourceType.VIRELIUM) &&
			resource.MaxAmount == ResourceCatalog.GetMaxAmount(ResourceType.VIRELIUM)
		)).IsTrue();

		context.AdvanceTick();

		AssertThat(context.get().Resources.Count).IsEqual(resourceCount);
		AssertThat(spawner.ResourceEntityIds.Count).IsEqual(resourceCount);
	}

	[TestCase]
	public void Push_GatherResources_AssignsCollectorToResourceState()
	{
		var context = new SimulationContext("match-1");
		var neutral = new PlayerState("neutral");
		var player = new PlayerState("player-1");
		context.AddPlayer(neutral);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"neutral",
			0,
			BuildingType.RESOURCE_SPAWNER,
			Vector2.Zero
		))).IsTrue();

		var resource = context.get().Resources.Values.First();
		var gatherer = new BuildingState(
			"gatherer-1",
			"player-1",
			resource.CurrentPosition + new Vector2(100, 0),
			BuildingType.RESOURCE_GATHERER,
			""
		);
		gatherer.AdvanceConstruction(100);
		player.AddEntity(gatherer);

		var collector = new ResourceCollectorState(
			"collector-1",
			"player-1",
			resource.CurrentPosition,
			UnitCatalog.GetMovementSpeed(UnitType.RESOURCE_COLLECTOR)
		);
		player.AddEntity(collector);

		var msg = new GatherResourcesMessage(
			"player-1",
			0,
			new[] { "collector-1" },
			resource.EntityId
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(collector.HasGatherOrder).IsTrue();
		AssertThat(collector.ResourceTargetId).IsEqual(resource.EntityId);
		AssertThat(collector.ResourceDropoffBuildingId).IsEqual(gatherer.EntityId);
		AssertThat(collector.GatherPhase).IsEqual(ResourceCollectorGatherPhase.MovingToResource);
		AssertThat(collector.HasMoveOrder).IsTrue();
		AssertThat(collector.TargetPosition).IsEqual(resource.CurrentPosition);

		context.AdvanceTick();

		AssertThat(collector.HasGatherOrder).IsTrue();
		AssertThat(collector.GatherPhase).IsEqual(ResourceCollectorGatherPhase.ReturningToDropoff);
		AssertThat(collector.Carry).IsEqual(collector.MaxCapacity);
		AssertThat(resource.CurrentAmount).IsEqual(ResourceCatalog.GetMaxAmount(ResourceType.VIRELIUM) - collector.MaxCapacity);
		AssertThat(collector.HasMoveOrder).IsTrue();
		var gathererHalfWidth = BuildingCatalog.GetFoodprintSize(BuildingType.RESOURCE_GATHERER).X / 2f;
		AssertThat(collector.TargetPosition).IsEqual(
			gatherer.CurrentPosition + Vector2.Left * (gathererHalfWidth + 10f));

		for (var i = 0; i < 5; i++)
			context.AdvanceTick();

		AssertThat(player.Virelium).IsEqual(collector.MaxCapacity);
		AssertThat(collector.Carry).IsEqual(0);
		AssertThat(collector.HasGatherOrder).IsTrue();
		AssertThat(collector.GatherPhase).IsEqual(ResourceCollectorGatherPhase.MovingToResource);
		AssertThat(collector.HasMoveOrder).IsTrue();
		AssertThat(collector.TargetPosition).IsEqual(resource.CurrentPosition);
	}

	[TestCase]
	public void Push_GatherResources_ReturnsFalse_WhenTargetIsResourceSpawnerBuilding()
	{
		var context = new SimulationContext("match-1");
		var neutral = new PlayerState("neutral");
		var player = new PlayerState("player-1");
		context.AddPlayer(neutral);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"neutral",
			0,
			BuildingType.RESOURCE_SPAWNER,
			Vector2.Zero
		))).IsTrue();

		var spawner = neutral.Entities.Values.OfType<BuildingState>().Single();
		var collector = new ResourceCollectorState(
			"collector-1",
			"player-1",
			Vector2.Zero,
			UnitCatalog.GetMovementSpeed(UnitType.RESOURCE_COLLECTOR)
		);
		player.AddEntity(collector);

		var msg = new GatherResourcesMessage(
			"player-1",
			0,
			new[] { "collector-1" },
			spawner.EntityId
		);

		AssertThat(context.Push(msg)).IsFalse();
		AssertThat(collector.HasGatherOrder).IsFalse();
	}

	[TestCase]
	public void Push_UseAbilityMessage_SellBuilding_RemovesCompletedBuilding()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var building = new BuildingState("building-1", "player-1", Vector2.Zero, BuildingType.BARRACKS, "builder-1");
		building.AdvanceConstruction(100);
		player.AddEntity(building);
		context.AddPlayer(player);

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "building-1" },
			p_ability_id: "sell_building"
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(player.Entities.ContainsKey("building-1")).IsFalse();
	}

	[TestCase]
	public void Push_UseAbilityMessage_CancelConstruction_RefundsSpentBuildingCost()
	{
		var context = new SimulationContext("match-1");
		var startingVirelium = 2000;
		var player = new PlayerState("player-1", startingVirelium);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		var buildCost = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT)
			.First(ability => ability.Id == "spawn_barracks")
			.Cost;

		var buildMessage = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_barracks",
			p_target_position: Vector2.Zero
		);

		AssertThat(context.Push(buildMessage)).IsTrue();
		AssertThat(player.Virelium).IsEqual(startingVirelium - buildCost);

		var site = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(site.ConstructionCost).IsEqual(buildCost);

		var cancelMessage = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { site.EntityId },
			p_ability_id: "cancel_construction"
		);

		AssertThat(context.Push(cancelMessage)).IsTrue();
		AssertThat(player.Entities.ContainsKey(site.EntityId)).IsFalse();
		AssertThat(player.Virelium).IsEqual(startingVirelium);
	}

	[TestCase]
	public void Push_UseAbilityMessage_SpawnPowerPlant_CreatesPowerPlantConstructionSite()
	{
		var context = new SimulationContext("match-1");
		var startingVirelium = 2000;
		var player = new PlayerState("player-1", startingVirelium);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		var buildCost = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT)
			.First(ability => ability.Id == "spawn_power_plant")
			.Cost;

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_power_plant",
			p_target_position: new Vector2(100, 50)
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(player.Virelium).IsEqual(startingVirelium - buildCost);

		var site = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(site.Type).IsEqual(BuildingType.CONSTRUCTION_SITE);
		AssertThat(site.pendingBuilding).IsEqual(BuildingType.POWER_PLANT);
		AssertThat(site.ConstructionCost).IsEqual(buildCost);
		AssertThat(builder.HasConstructionOrder).IsTrue();
		AssertThat(builder.ConstructionTargetId).IsEqual(site.EntityId);
	}

	[TestCase]
	public void Push_UseAbilityMessage_SpawnWarFactory_CreatesWarFactoryConstructionSite()
	{
		var context = new SimulationContext("match-1");
		var startingVirelium = 2000;
		var player = new PlayerState("player-1", startingVirelium);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		var buildCost = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT)
			.First(ability => ability.Id == "spawn_war_factory")
			.Cost;

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_war_factory",
			p_target_position: new Vector2(100, 50)
		);

		AssertThat(player.UnlockedAbilities.Contains("spawn_war_factory")).IsFalse();
		AssertThat(context.Push(msg)).IsFalse();
		AssertThat(player.Virelium).IsEqual(startingVirelium);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.RESOURCE_GATHERER,
			new Vector2(-200, 0)
		))).IsTrue();

		AssertThat(player.UnlockedAbilities.Contains("spawn_war_factory")).IsTrue();
		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(player.Virelium).IsEqual(startingVirelium - buildCost);

		var site = player.Entities.Values
			.OfType<BuildingState>()
			.Single(building => building.Type == BuildingType.CONSTRUCTION_SITE);
		AssertThat(site.Type).IsEqual(BuildingType.CONSTRUCTION_SITE);
		AssertThat(site.pendingBuilding).IsEqual(BuildingType.WAR_FACTORY);
		AssertThat(site.ConstructionCost).IsEqual(buildCost);
		AssertThat(builder.HasConstructionOrder).IsTrue();
		AssertThat(builder.ConstructionTargetId).IsEqual(site.EntityId);
	}

	[TestCase]
	public void Push_UseAbilityMessage_SellLastSupplyCenterLocksWarFactory()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.RESOURCE_GATHERER,
			Vector2.Zero
		))).IsTrue();

		var supplyCenter = player.Entities.Values
			.OfType<BuildingState>()
			.Single(building => building.Type == BuildingType.RESOURCE_GATHERER);

		AssertThat(player.UnlockedAbilities.Contains("spawn_war_factory")).IsTrue();

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { supplyCenter.EntityId },
			p_ability_id: "sell_building"
		))).IsTrue();

		AssertThat(player.UnlockedAbilities.Contains("spawn_war_factory")).IsFalse();
	}

	[TestCase]
	public void Push_UseAbilityMessage_SpawnBuildingRejectsOverlappingBuilding()
	{
		var context = new SimulationContext("match-1");
		var startingVirelium = 2000;
		var player = new PlayerState("player-1", startingVirelium);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.COMMAND_CENTER,
			new Vector2(100, 100)
		))).IsTrue();

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_barracks",
			p_target_position: new Vector2(100, 100)
		);

		AssertThat(context.Push(msg)).IsFalse();
		AssertThat(player.Virelium).IsEqual(startingVirelium);
		AssertThat(player.Entities.Values.OfType<BuildingState>().Count()).IsEqual(1);
	}

	[TestCase]
	public void Push_UseAbilityMessage_SpawnBuildingAllowsTouchingFootprintEdges()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1", 2000);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.BARRACKS,
			Vector2.Zero
		))).IsTrue();

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_barracks",
			p_target_position: new Vector2(120, 0)
		);

		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(player.Entities.Values.OfType<BuildingState>().Count()).IsEqual(2);
	}

	[TestCase]
	public void Push_DebugSpawnBuilding_PowerPlant_AddsTenProducedEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.POWER_PLANT,
			Vector2.Zero
		))).IsTrue();

		AssertThat(player.EnergyProduced).IsEqual(10);
	}

	[TestCase]
	public void AdvanceTick_CompletedPowerPlant_AddsTenProducedEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_power_plant",
			p_target_position: Vector2.Zero
		))).IsTrue();

		var site = player.Entities.Values.OfType<BuildingState>().Single();
		site.AdvanceConstruction(95);
		builder.ClearMoveOrder();

		context.AdvanceTick();

		AssertThat(site.Type).IsEqual(BuildingType.POWER_PLANT);
		AssertThat(player.EnergyProduced).IsEqual(10);
	}

	[TestCase]
	public void Push_UseAbilityMessage_SellPowerPlant_RemovesProducedEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.POWER_PLANT,
			Vector2.Zero
		))).IsTrue();
		var building = player.Entities.Values.OfType<BuildingState>().Single();

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { building.EntityId },
			p_ability_id: "sell_building"
		))).IsTrue();

		AssertThat(player.EnergyProduced).IsEqual(0);
	}

	[TestCase]
	public void Push_DebugSpawnBuilding_WarFactory_AddsTwoConsumedEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.WAR_FACTORY,
			Vector2.Zero
		))).IsTrue();

		var building = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(player.EnergyConsumed).IsEqual(2);
		AssertThat(building.Abilities.Count).IsEqual(1);
		AssertThat(building.Abilities.Any(ability => ability.Id == "sell_building")).IsTrue();
	}

	[TestCase]
	public void Push_UseAbilityMessage_SellWarFactory_RemovesConsumedEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.WAR_FACTORY,
			Vector2.Zero
		))).IsTrue();
		var building = player.Entities.Values.OfType<BuildingState>().Single();

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { building.EntityId },
			p_ability_id: "sell_building"
		))).IsTrue();

		AssertThat(player.EnergyConsumed).IsEqual(0);
	}

	[TestCase]
	public void AdvanceTick_LightTankProductionPausesDuringPowerDeficitAndResumes()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1", 2000);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.WAR_FACTORY,
			Vector2.Zero
		))).IsTrue();

		var warFactory = player.Entities.Values
			.OfType<BuildingState>()
			.Single(building => building.Type == BuildingType.WAR_FACTORY);

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { warFactory.EntityId },
			p_ability_id: "spawn_light_tank"
		))).IsTrue();

		AssertThat(warFactory.ProductionQueue).ContainsExactly(UnitType.LIGHT_TANK);
		AssertThat(player.EnergyProduced).IsEqual(0);
		AssertThat(player.EnergyConsumed).IsEqual(2);

		context.AdvanceTick();

		AssertThat(warFactory.ProductionProgress).IsEqual(0);
		AssertThat(warFactory.ProductionQueue).ContainsExactly(UnitType.LIGHT_TANK);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			1,
			BuildingType.POWER_PLANT,
			new Vector2(200, 0)
		))).IsTrue();

		context.AdvanceTick();

		AssertThat(warFactory.ProductionProgress).IsEqual(5);
		AssertThat(warFactory.ProductionQueue).ContainsExactly(UnitType.LIGHT_TANK);

		var powerPlant = player.Entities.Values
			.OfType<BuildingState>()
			.Single(building => building.Type == BuildingType.POWER_PLANT);

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 2,
			p_caster_entity_ids: new[] { powerPlant.EntityId },
			p_ability_id: "sell_building"
		))).IsTrue();

		context.AdvanceTick();

		AssertThat(warFactory.ProductionProgress).IsEqual(5);
		AssertThat(warFactory.ProductionQueue).ContainsExactly(UnitType.LIGHT_TANK);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			3,
			BuildingType.POWER_PLANT,
			new Vector2(200, 0)
		))).IsTrue();

		context.AdvanceTick();

		AssertThat(warFactory.ProductionProgress).IsEqual(10);
		AssertThat(warFactory.ProductionQueue).ContainsExactly(UnitType.LIGHT_TANK);
	}

	[TestCase]
	public void AdvanceTick_LightTankCompletesWhenPlayerHasSufficientEnergy()
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1", 2000);
		context.AddPlayer(player);

		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.WAR_FACTORY,
			Vector2.Zero
		))).IsTrue();
		AssertThat(context.Push(new DebugSpawnBuildingMessage(
			"player-1",
			0,
			BuildingType.POWER_PLANT,
			new Vector2(200, 0)
		))).IsTrue();

		var warFactory = player.Entities.Values
			.OfType<BuildingState>()
			.Single(building => building.Type == BuildingType.WAR_FACTORY);

		AssertThat(context.Push(new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { warFactory.EntityId },
			p_ability_id: "spawn_light_tank"
		))).IsTrue();

		for (var i = 0; i < UnitCatalog.GetProductionTime(UnitType.LIGHT_TANK) / 5; i++)
			context.AdvanceTick();

		var lightTank = player.Entities.Values
			.OfType<UnitState>()
			.Single(unit => unit.Type == UnitType.LIGHT_TANK);
		var expectedSpawnPoint = warFactory.CurrentPosition
			+ BuildingCatalog.GetFoodprintSize(BuildingCatalog.GetFootprintType(warFactory)) / 2f
			+ new Vector2(5, 5);

		AssertThat(warFactory.ProductionQueue).IsEmpty();
		AssertThat(lightTank.CurrentPosition).IsEqual(expectedSpawnPoint);
		AssertThat(lightTank.HasMoveOrder).IsTrue();
		AssertThat(lightTank.TargetPosition).IsEqual(warFactory.RallyPoint);
		AssertThat(lightTank.MaxHealth).IsEqual(450);
		AssertThat(lightTank.WeaponClass).IsEqual(WeaponClass.CANNON);
	}

	[TestCase]
	public void Push_UseAbilityMessage_ReturnsFalse_WhenAbilityIsLockedForPlayer()
	{
		var context = new SimulationContext("match-1");
		var startingVirelium = 2000;
		var player = new PlayerState("player-1", startingVirelium);
		var builder = new UnitState("builder-1", "player-1", Vector2.Zero, 100f, UnitType.CONSTRUCTION_UNIT);
		player.AddEntity(builder);
		context.AddPlayer(player);

		AssertThat(AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT)
			.First(ability => ability.Id == "spawn_resource_gatherer")
			.UnlockedFromStart).IsFalse();
		AssertThat(player.UnlockedAbilities.Contains("spawn_resource_gatherer")).IsFalse();
		AssertThat(player.AbilityUnlockRevision).IsEqual(0);

		var msg = new UseAbilityMessage(
			p_player_id: "player-1",
			p_issued_at_tick: 0,
			p_caster_entity_ids: new[] { "builder-1" },
			p_ability_id: "spawn_resource_gatherer",
			p_target_position: Vector2.Zero
		);

		AssertThat(context.Push(msg)).IsFalse();
		AssertThat(player.Virelium).IsEqual(startingVirelium);
		AssertThat(player.Entities.Values.OfType<BuildingState>().Any()).IsFalse();

		player.UnlockAbility("spawn_resource_gatherer");

		AssertThat(player.AbilityUnlockRevision).IsEqual(1);
		AssertThat(context.Push(msg)).IsTrue();
		AssertThat(player.Virelium).IsEqual(startingVirelium - 1500);

		var site = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(site.pendingBuilding).IsEqual(BuildingType.RESOURCE_GATHERER);
	}

	private static MoveUnitsMessage CreateMoveMessage(string playerId, string unitId, Vector2 destination)
	{
		return new MoveUnitsMessage(
			p_player_id: playerId,
			p_issued_at_tick: 0,
			p_unit_ids: new[] { unitId },
			p_destination: destination
		);
	}

	private static DebugSpawnUnitsMessage CreateSpawnMessage(string playerId, Vector2 position)
	{
		return new DebugSpawnUnitsMessage(
			playerId,
			0,
			UnitType.BASIC_INFANTRY,
			position,
			position,
			UnitCatalog.GetMovementSpeed(UnitType.BASIC_INFANTRY)
		);
	}

	private static int ExpectedDamage(UnitState attacker, EntityState target)
	{
		var modifier = WeaponEffectivenessCatalog.GetModifier(attacker.WeaponClass, target.ArmorClass);
		if (modifier <= 0f)
			return 0;

		return System.Math.Max(1, (int)System.Math.Round(attacker.AttackDamage * modifier, System.MidpointRounding.AwayFromZero));
	}

	private static bool IsInsideBuildingFootprint(Vector2 point, BuildingState building)
	{
		var footprintSize = BuildingCatalog.GetFoodprintSize(BuildingCatalog.GetFootprintType(building));
		var halfSize = footprintSize / 2f;
		return point.X >= building.CurrentPosition.X - halfSize.X
			&& point.X <= building.CurrentPosition.X + halfSize.X
			&& point.Y >= building.CurrentPosition.Y - halfSize.Y
			&& point.Y <= building.CurrentPosition.Y + halfSize.Y;
	}

	private static void AssertMoveRoutesAroundBuilding(Vector2 start, Vector2 destination, System.Func<Vector2, bool> movedOffAxis)
	{
		var context = new SimulationContext("match-1");
		var player = new PlayerState("player-1");
		var unit = new UnitState("unit-1", "player-1", start, 100f);
		var building = new BuildingState("building-1", "player-1", new Vector2(100, 0), BuildingType.BARRACKS, "");
		building.AdvanceConstruction(100);
		player.AddEntity(unit);
		player.AddEntity(building);
		context.AddPlayer(player);

		AssertThat(context.Push(CreateMoveMessage("player-1", "unit-1", destination))).IsTrue();

		var movedOffDirectLine = false;
		for (var i = 0; i < 30 && unit.HasMoveOrder; i++)
		{
			context.AdvanceTick();
			movedOffDirectLine |= movedOffAxis(unit.CurrentPosition);
			AssertThat(IsInsideBuildingFootprint(unit.CurrentPosition, building)).IsFalse();
		}

		AssertThat(movedOffDirectLine).IsTrue();
		AssertThat(unit.HasMoveOrder).IsFalse();
		AssertThat(unit.CurrentPosition).IsEqual(destination);
	}
}
