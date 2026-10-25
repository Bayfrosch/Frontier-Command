using Godot;
using GdUnit4;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class SquadIconSelectionTest
{
	[TestCase]
	public async Task ClickingSquadIconsSelectsMembersAndSupportsShiftSelection()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = (GameScene)runner.Scene();
		scene.GetNode<TimeTickSystem>("GameLoop").SetProcess(false);
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var renderer = scene.GetNode<ClientWorldRenderer>("WorldRenderer");

		foreach (var (squadId, position) in new[]
		{
			("squad-1", new Vector2(-600, -600)),
			("squad-2", new Vector2(-300, -600))
		})
		{
			AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
				"player_1", 0, UnitType.BASIC_INFANTRY, position, position, 100f, squadId))).IsTrue();
			AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
				"player_1", 0, UnitType.BASIC_INFANTRY, position + new Vector2(40, 0),
				position + new Vector2(40, 0), 100f, squadId))).IsTrue();
		}

		await runner.SimulateFrames(2);
		var icons = renderer.GetChildren().OfType<ClientSquadState>().ToDictionary(icon => icon.SquadId);
		var squads = simulation.GetState().Players["player_1"].Squads;
		AssertThat(icons["squad-1"].GetNode<TextureRect>("Icon").MouseFilter)
			.IsEqual(Control.MouseFilterEnum.Ignore);

		scene.Call("SelectAtWorldPosition", icons["squad-1"].GlobalPosition, false);
		AssertThat(scene.SelectedEntityIds.ToHashSet().SetEquals(squads["squad-1"].Soldiers.Keys)).IsTrue();

		scene.Call("SelectAtWorldPosition", icons["squad-2"].GlobalPosition, true);
		AssertThat(scene.SelectedEntityIds.ToHashSet().SetEquals(
			squads.Values.SelectMany(squad => squad.Soldiers.Keys))).IsTrue();
	}
}
