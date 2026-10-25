using Godot;
using GdUnit4;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ResearchBranchTest
{
	[TestCase]
	public async Task BranchLayoutAndRepeatPurchaseReflectTheRealResearchState()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		scene.GetNode<TimeTickSystem>("GameLoop").SetProcess(false);
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var player = simulation.GetState().Players["player_1"];
		var overlay = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface").Research;
		player.AddResearch("infantry_training");
		player.AddResearch("anti_armor_infantry");
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		overlay.Show();
		await runner.SimulateFrames(2);
		var tree = overlay.GetNode<Control>("ResearchPanel/Margin/Layout/Body/TreeScroll/TreeCenter/MilitaryTree");
		var recruitment = tree.GetNode<Button>("infantry_training");
		var weapons = tree.GetNode<Button>("better_weapons");
		var rpg = tree.GetNode<Button>("anti_armor_infantry");
		var support = tree.GetNode<Button>("support_equipment");
		AssertThat(weapons.Position.X < recruitment.Position.X).IsTrue();
		AssertThat(weapons.Position.Y).IsEqual(recruitment.Position.Y);
		AssertThat(rpg.Position.X).IsEqual(recruitment.Position.X);
		AssertThat(support.Position.X).IsEqual(rpg.Position.X);
		AssertThat(support.Position.Y > rpg.Position.Y).IsTrue();
		AssertThat(tree.GetNode<Label>("Generation1Label").GetRect().End.Y < recruitment.Position.Y).IsTrue();
		var cards = tree.GetChildren().OfType<Button>().ToArray();
		foreach (var card in cards)
		{
			AssertThat(card.Size).IsEqual(new Vector2(240, 64));
			AssertThat(card.Text.Contains("Materials")).IsFalse();
			foreach (var other in cards.Where(other => other != card))
				AssertThat(card.GetRect().Intersects(other.GetRect())).IsFalse();
		}
		AssertThat(support.Text.Contains("0/2")).IsTrue();
		support.EmitSignal(Button.SignalName.Pressed);
		var detail = overlay.GetNode<Control>("ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content");
		var start = detail.GetNode<Button>("StartButton");
		var cost = detail.GetNode<Label>("Cost");
		for (var level = 1; level <= 2; level++)
		{
			AssertThat(start.Disabled).IsFalse();
			AssertThat(start.Text).IsEqual($"Research level {level}");
			AssertThat(cost.Text.Contains((level == 1 ? 1400 : 1800).ToString("N0"))).IsTrue();
			start.EmitSignal(Button.SignalName.Pressed);
			AssertThat(support.Text.Contains("RESEARCHING")).IsTrue();
			var duration = player.ResearchProjects["support_equipment"].DurationTicks;
			for (var tick = 0; tick < duration; tick++)
				simulation.Context.AdvanceTick();
			simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
			AssertThat(support.Text.Contains($"{level}/2")).IsTrue();
		}
		AssertThat(start.Disabled).IsTrue();
		AssertThat(support.Text.Contains("COMPLETE")).IsTrue();
		AssertThat(cost.Text).IsEqual("All 2 levels researched.");
	}
}
