using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class SimulationCoreTest
{
    [TestCase]
    public void Push_ReturnsFalse_WhenMessageIsNotACommand()
    {
        var context = new SimulationContext("match-1");
        var msg = new PingMessage(p_client_time_unix_ms: 123);

        AssertThat(context.push(msg)).IsFalse();
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

        AssertThat(context.push(msg)).IsFalse();
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

        AssertThat(context.push(msg)).IsFalse();
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

        AssertThat(context.push(msg)).IsFalse();
    }

    [TestCase]
    public void Push_MoveUnitsMessage_ReturnsFalse_WhenEntityDoesNotExist()
    {
        var context = new SimulationContext("match-1");
        var player = new PlayerState("player-1");
        context.AddPlayer(player);

        var msg = CreateMoveMessage("player-1", "missing-unit", new Vector2(1, 2));

        AssertThat(context.push(msg)).IsFalse();
    }

    [TestCase]
    public void Push_MoveUnitsMessage_ReturnsFalse_WhenEntityIsNotAUnit()
    {
        var context = new SimulationContext("match-1");
        var player = new PlayerState("player-1");
        var building = new BuildingState("building-1", "player-1", Vector2.Zero, BuildingType.BASIC_GENERATOR);
        player.AddEntity(building);
        context.AddPlayer(player);

        var msg = CreateMoveMessage("player-1", "building-1", new Vector2(1, 2));

        AssertThat(context.push(msg)).IsFalse();
    }

    [TestCase]
    public void Push_MoveUnitsMessage_AssignsMoveOrderToUnit()
    {
        var context = new SimulationContext("match-1");
        var player = new PlayerState("player-1");
        var unit = new UnitState("unit-1", "player-1", Vector2.Zero);
        player.AddEntity(unit);
        context.AddPlayer(player);

        var destination = new Vector2(10, 20);
        var msg = CreateMoveMessage("player-1", "unit-1", destination);

        AssertThat(context.push(msg)).IsTrue();
        AssertThat(unit.HasMoveOrder).IsTrue();
        AssertThat(unit.TargetPosition).IsEqual(destination);
    }

    [TestCase]
    public void Push_MoveUnitsMessage_AssignsMoveOrderToEverySelectedUnit()
    {
        var context = new SimulationContext("match-1");
        var player = new PlayerState("player-1");
        var unitA = new UnitState("unit-a", "player-1", Vector2.Zero);
        var unitB = new UnitState("unit-b", "player-1", Vector2.Zero);
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

        AssertThat(context.push(msg)).IsTrue();
        AssertThat(unitA.HasMoveOrder).IsTrue();
        AssertThat(unitA.TargetPosition).IsEqual(destination);
        AssertThat(unitB.HasMoveOrder).IsTrue();
        AssertThat(unitB.TargetPosition).IsEqual(destination);
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
}
