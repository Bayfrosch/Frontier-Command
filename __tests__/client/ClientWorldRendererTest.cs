using Godot;
using GdUnit4;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ClientWorldRendererTest
{
	[TestCase]
	public async Task SquadIconSpawnsAtCenterAndMovesSmoothly()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		scene.GetNode<TimeTickSystem>("GameLoop").SetProcess(false);
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var renderer = scene.GetNode<ClientWorldRenderer>("WorldRenderer");
		var position = new Vector2(600, 600);

		AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
			"player_1", 0, UnitType.BASIC_INFANTRY, position, position, 100f, "squad-1"))).IsTrue();
		AssertThat(renderer.GetChildren().OfType<ClientUnit>()
			.Count(unit => unit.SquadId == "squad-1")).IsEqual(1);
		var icon = renderer.GetChildren().OfType<ClientSquadState>().Single();
		AssertThat(icon.SquadId).IsEqual("squad-1");
		AssertThat(icon.GlobalPosition).IsEqual(position);

		var secondPosition = new Vector2(640, 600);
		AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
			"player_1", 0, UnitType.BASIC_INFANTRY, secondPosition, secondPosition, 100f, "squad-1"))).IsTrue();
		AssertThat(renderer.GetChildren().OfType<ClientSquadState>().Count()).IsEqual(1);
		AssertThat(icon.GlobalPosition).IsEqual(position);
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(610, 600));
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(620, 600));

		var player = simulation.GetState().Players["player_1"];
		foreach (var unit in player.Squads["squad-1"].Soldiers.Values)
		{
			unit.SetMoveOrder(unit.CurrentPosition + new Vector2(20, 0));
			unit.AdvanceMovement(0.2f);
		}
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(620, 600));
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(630, 600));
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(640, 600));

		foreach (var unitId in player.Squads["squad-1"].Soldiers.Keys.ToArray())
			player.RemoveEntity(unitId);
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		await runner.SimulateFrames(2);
		AssertThat(renderer.GetChildren().OfType<ClientSquadState>().Count()).IsEqual(0);
	}
}
