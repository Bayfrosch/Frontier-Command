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
		AssertThat(unitA.TargetPosition).IsEqual(destination);
		AssertThat(unitB.HasMoveOrder).IsTrue();
		AssertThat(unitB.TargetPosition).IsEqual(destination);
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
		AssertThat(units.Any(unit => unit.TargetPosition == position + new Vector2(32f, 0f))).IsTrue();
		AssertThat(units.Any(unit => unit.TargetPosition == position + new Vector2(-32f, 0f))).IsTrue();
	}

	[TestCase]
	public void AdvanceTick_BasicInfantryDamagesAttackTarget_AfterWindup_WhenEnemyIsInRange()
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

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth);

		context.AdvanceTick();
		AssertThat(target.Health).IsEqual(target.MaxHealth - UnitCatalog.GetAttackDamage(UnitType.BASIC_INFANTRY));
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
			UnitCatalog.GetMovementSpeed(UnitType.BASIC_INFANTRY)
		);
	}
}
