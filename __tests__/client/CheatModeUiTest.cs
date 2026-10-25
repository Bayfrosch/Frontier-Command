using Godot;
using GdUnit4;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class CheatModeUiTest
{
	[TestCase]
	public async Task SettingsButtonReflectsAndTogglesLocalPlayerCheatMode()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var button = scene.GetNode<Button>(
			"Camera2D/Screen/GameUserInterface/Settings/SettingsPanel/MarginContainer/VBoxContainer/CheatModeButton");

		AssertThat(button.Text).IsEqual("Cheat Mode: Disabled");
		AssertThat(button.ButtonPressed).IsFalse();
		button.EmitSignal(Button.SignalName.Pressed);
		AssertThat(simulation.GetState().Players["player_1"].CheatModeEnabled).IsTrue();
		AssertThat(button.Text).IsEqual("Cheat Mode: Enabled");
		AssertThat(button.ButtonPressed).IsTrue();

		button.EmitSignal(Button.SignalName.Pressed);
		AssertThat(simulation.GetState().Players["player_1"].CheatModeEnabled).IsFalse();
		AssertThat(button.Text).IsEqual("Cheat Mode: Disabled");
		AssertThat(button.ButtonPressed).IsFalse();
	}
}
