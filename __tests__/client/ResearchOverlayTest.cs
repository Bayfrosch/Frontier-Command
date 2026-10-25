using Godot;
using GdUnit4;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ResearchOverlayTest
{
	private const string StartPath = "ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content/StartButton";
	private const string CancelPath = "ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content/CancelButton";

	private static void Click(Viewport viewport, Control control) => Click(viewport, control.GetGlobalRect().GetCenter());

	private static void Click(Viewport viewport, Vector2 point)
	{
		viewport.PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
		viewport.PushInput(new InputEventMouseButton
		{
			Position = point,
			GlobalPosition = point,
			ButtonIndex = MouseButton.Left,
			Pressed = true,
			ButtonMask = MouseButtonMask.Left
		}, true);
		viewport.PushInput(new InputEventMouseButton
		{
			Position = point,
			GlobalPosition = point,
			ButtonIndex = MouseButton.Left,
			Pressed = false
		}, true);
	}

	[TestCase]
	public async Task PowerDeficit_PausesDisplayedTimer_AndRestoredPowerResumesIt()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var overlay = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface").Research;
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var player = simulation.GetState().Players["player_1"];
		overlay.Show();
		overlay.GetNode<Button>("ResearchPanel/Margin/Layout/Body/TreeScroll/TreeCenter/MilitaryTree/light_tanks")
			.EmitSignal(Button.SignalName.Pressed);
		overlay.GetNode<Button>(StartPath).EmitSignal(Button.SignalName.Pressed);
		var status = overlay.GetNode<Label>("ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content/Status");
		AssertThat(status.Text).IsEqual("Paused: insufficient energy (60s remaining)");
		for (var tick = 0; tick < 20; tick++)
			simulation.Context.AdvanceTick();
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(status.Text).IsEqual("Paused: insufficient energy (60s remaining)");
		AssertThat(player.ResearchProjects["light_tanks"].ElapsedTicks).IsEqual(0);

		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		for (var tick = 0; tick < 5; tick++)
			simulation.Context.AdvanceTick();
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(status.Text).IsEqual("Researching: 59s remaining");
	}

	[TestCase]
	public async Task ChapterTabs_SelectCivilianAndMilitaryTrees_AfterScrolling()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var viewport = scene.GetViewport();
		var ui = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface");
		var camera = scene.GetNode<CameraController>("Camera2D");
		Click(viewport, ui.GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/ResearchButton"));
		await runner.SimulateFrames(2);
		var scroll = ui.Research.GetNode<ScrollContainer>("ResearchPanel/Margin/Layout/Body/TreeScroll");
		var title = ui.Research.GetNode<Label>("ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content/TechnologyName");
		var tabs = ui.Research.GetNode<TabBar>("ResearchPanel/Margin/Layout/Chapters");
		AssertThat(tabs.TabCount).IsEqual(3);
		Click(viewport, tabs.GlobalPosition + tabs.GetTabRect(1).GetCenter());
		await runner.SimulateFrames(2);
		Click(viewport, scroll.GetNode<Button>("TreeCenter/CivilianTree/refining"));
		AssertThat(title.Text).IsEqual("Efficient refining");
		AssertThat(scroll.GetNode<Control>("TreeCenter/MilitaryTree").Visible).IsFalse();
		AssertThat(scroll.GetNode<Control>("TreeCenter/CivilianTree").Visible).IsTrue();
		Click(viewport, tabs.GlobalPosition + tabs.GetTabRect(0).GetCenter());
		await runner.SimulateFrames(2);
		var zoom = camera.Zoom;
		var point = scroll.GetGlobalRect().GetCenter();
		viewport.PushInput(new InputEventMouseButton
		{
			Position = point,
			GlobalPosition = point,
			ButtonIndex = MouseButton.WheelDown,
			Pressed = true
		}, true);
		viewport.PushInput(new InputEventMouseButton
		{
			Position = point,
			GlobalPosition = point,
			ButtonIndex = MouseButton.WheelDown,
			Pressed = false
		}, true);
		await runner.SimulateFrames(2);
		AssertThat(scroll.ScrollVertical > 0).IsTrue();
		AssertThat(camera.Zoom).IsEqual(zoom);
		scroll.ScrollVertical = (int)scroll.GetNode<Button>("TreeCenter/MilitaryTree/infantry_doctrine").Position.Y - 80;
		await runner.SimulateFrames(2);
		Click(viewport, scroll.GetNode<Button>("TreeCenter/MilitaryTree/infantry_doctrine"));
		AssertThat(title.Text).IsEqual("Infantry doctrine");
		AssertThat(ui.Research.GetNode<Button>(StartPath).Disabled).IsTrue();
		tabs.CurrentTab = 2;
		await runner.SimulateFrames(2);
		AssertThat(title.Text).IsEqual("Fleet capacity I");
		AssertThat(scroll.GetNode<Control>("TreeCenter/FleetTree").Visible).IsTrue();
		viewport.PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = true }, true);
		AssertThat(ui.Research.Visible).IsFalse();
	}

	[TestCase]
	public async Task OpenAndClose_BlockWorldInput_WhileSimulationContinues()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var ui = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface");
		var camera = scene.GetNode<CameraController>("Camera2D");
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		AssertThat(ui.Research.Visible).IsFalse();
		ui.GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/ResearchButton").EmitSignal(Button.SignalName.Pressed);
		await runner.SimulateFrames(2);
		AssertThat(ui.Research.Visible).IsTrue();
		AssertThat(scene.IsProcessingInput()).IsFalse();
		AssertThat(scene.IsProcessingUnhandledInput()).IsFalse();
		AssertThat(camera.IsProcessingInput()).IsFalse();
		var tick = simulation.GetState().Tick;
		await scene.ToSignal(scene.GetTree().CreateTimer(0.25), SceneTreeTimer.SignalName.Timeout);
		AssertThat(simulation.GetState().Tick > tick).IsTrue();
		ui._UnhandledKeyInput(new InputEventKey { Keycode = Key.Escape, Pressed = true });
		AssertThat(ui.Research.Visible).IsFalse();
		AssertThat(scene.IsProcessingInput()).IsTrue();
		AssertThat(scene.IsProcessingUnhandledInput()).IsTrue();
		AssertThat(camera.IsProcessingInput()).IsTrue();
	}

	[TestCase]
	public async Task StartAndCancel_UpdateRealMaterials_AndSurviveClosingOverlay()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var ui = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface");
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var player = simulation.GetState().Players["player_1"];
		var openingBalance = player.Virelium;
		ui.GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/ResearchButton").EmitSignal(Button.SignalName.Pressed);
		await runner.SimulateFrames(2);
		Click(scene.GetViewport(), ui.Research.GetNode<Button>("ResearchPanel/Margin/Layout/Body/TreeScroll/TreeCenter/MilitaryTree/light_tanks"));
		AssertThat(player.Virelium).IsEqual(openingBalance);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		await runner.SimulateFrames(2);
		ui.Research.GetNode<ScrollContainer>("ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll")
			.EnsureControlVisible(ui.Research.GetNode<Button>(StartPath));
		await runner.SimulateFrames(2);
		Click(scene.GetViewport(), ui.Research.GetNode<Button>(StartPath));
		AssertThat(player.Virelium).IsEqual(openingBalance - 1000);
		AssertThat(player.ResearchProjects.ContainsKey("light_tanks")).IsTrue();
		ui.Research.Close();
		await runner.SimulateFrames(3);
		ui.Research.Show();
		AssertThat(ui.Research.GetNode<Button>(CancelPath).Visible).IsTrue();
		ui.Research.GetNode<Button>(CancelPath).EmitSignal(Button.SignalName.Pressed);
		AssertThat(player.Virelium).IsEqual(openingBalance);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
	}

	[TestCase]
	public async Task Chapters_KeepIndependentProjects_AndAllowRefundsAfterSwitching()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var overlay = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface").Research;
		var player = scene.GetNode<LocalSimulationNode>("SimulationCore").GetState().Players["player_1"];
		var balance = player.Virelium;
		overlay.Show();
		var tabs = overlay.GetNode<TabBar>("ResearchPanel/Margin/Layout/Chapters");
		for (var chapter = 0; chapter < 3; chapter++)
		{
			tabs.CurrentTab = chapter;
			overlay.GetNode<Button>(StartPath).EmitSignal(Button.SignalName.Pressed);
		}
		AssertThat(player.ResearchProjects.Count).IsEqual(3);
		AssertThat(player.Virelium).IsEqual(balance - 2600);
		for (var chapter = 0; chapter < 3; chapter++)
		{
			tabs.CurrentTab = chapter;
			AssertThat(overlay.GetNode<Button>(CancelPath).Visible).IsTrue();
			overlay.GetNode<Button>(CancelPath).EmitSignal(Button.SignalName.Pressed);
		}
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.Virelium).IsEqual(balance);
	}

	[TestCase]
	public async Task DoctrineAction_EnablesOnlyAfterRequiredTreesAreComplete()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var overlay = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface").Research;
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var player = simulation.GetState().Players["player_1"];
		overlay.Show();
		overlay.GetNode<Button>("ResearchPanel/Margin/Layout/Body/TreeScroll/TreeCenter/MilitaryTree/infantry_doctrine")
			.EmitSignal(Button.SignalName.Pressed);
		AssertThat(overlay.GetNode<Button>(StartPath).Disabled).IsTrue();
		foreach (var technology in ResearchCatalog.All)
		{
			if (technology.Generation == 1 && (technology.Chapter == ResearchChapter.Military && technology.Tree != "Tanks"
				|| technology.Chapter == ResearchChapter.Civilian && technology.Tree == "Extraction"))
				player.AddResearch(technology.Id);
		}
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(overlay.GetNode<Button>(StartPath).Disabled).IsFalse();
		var banner = overlay.GetNode<Label>("ResearchPanel/Margin/Layout/DoctrinePanel/DoctrineProgress").Text;
		AssertThat(banner.Contains("Military 2/2")).IsTrue();
		AssertThat(banner.Contains("Civilian 1/1")).IsTrue();
		AssertThat(banner.Contains("permanently excludes the other")).IsTrue();
	}

	[TestCase]
	public async Task DoctrineChoice_DimsOtherPath_ReopensOnCancel_AndExcludesOnCompletion()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var overlay = scene.GetNode<GameUserInterface>("Camera2D/Screen/GameUserInterface").Research;
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var player = simulation.GetState().Players["player_1"];
		var balance = player.Virelium;
		foreach (var technology in ResearchCatalog.All)
		{
			if (technology.Generation == 1 && (technology.Chapter == ResearchChapter.Military && technology.Tree != "Tanks"
				|| technology.Chapter == ResearchChapter.Civilian && technology.Tree == "Extraction"))
				player.AddResearch(technology.Id);
		}
		overlay.Show();
		var tree = overlay.GetNode<Control>("ResearchPanel/Margin/Layout/Body/TreeScroll/TreeCenter/MilitaryTree");
		var infantry = tree.GetNode<Button>("infantry_doctrine");
		var tanks = tree.GetNode<Button>("tank_doctrine");
		var originalColor = tanks.Modulate;
		AssertThat(infantry.Text.Contains("EXCLUDES TANKS")).IsTrue();
		AssertThat(tanks.Text.Contains("EXCLUDES INFANTRY")).IsTrue();
		infantry.EmitSignal(Button.SignalName.Pressed);
		AssertThat(overlay.GetNode<Button>(StartPath).Text).IsEqual("Choose Infantry doctrine");
		overlay.GetNode<Button>(StartPath).EmitSignal(Button.SignalName.Pressed);
		AssertThat(tanks.Text.Contains("OTHER PATH PENDING")).IsTrue();
		AssertThat(tanks.Modulate.R < originalColor.R).IsTrue();
		AssertThat(tree.GetNode<Button>("battle_tanks").Text.Contains("OTHER PATH PENDING")).IsTrue();
		tanks.EmitSignal(Button.SignalName.Pressed);
		AssertThat(overlay.GetNode<Button>(StartPath).Disabled).IsTrue();
		infantry.EmitSignal(Button.SignalName.Pressed);
		overlay.GetNode<Button>(CancelPath).EmitSignal(Button.SignalName.Pressed);
		AssertThat(player.Virelium).IsEqual(balance);
		AssertThat(tanks.Text.Contains("EXCLUDES INFANTRY")).IsTrue();
		AssertThat(tanks.Modulate).IsEqual(originalColor);
		tanks.EmitSignal(Button.SignalName.Pressed);
		overlay.GetNode<Button>(StartPath).EmitSignal(Button.SignalName.Pressed);
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		for (var tick = 0; tick < 600; tick++)
			simulation.Context.AdvanceTick();
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(player.Research.ContainsKey("tank_doctrine")).IsTrue();
		AssertThat(infantry.Text.Contains("EXCLUDED")).IsTrue();
		AssertThat(tree.GetNode<Button>("assault_infantry").Text.Contains("EXCLUDED")).IsTrue();
		AssertThat(tree.GetNode<Button>("light_tanks").Modulate).IsEqual(originalColor);
		infantry.EmitSignal(Button.SignalName.Pressed);
		AssertThat(overlay.GetNode<Button>(StartPath).Disabled).IsTrue();
		AssertThat(overlay.GetNode<Label>("ResearchPanel/Margin/Layout/DoctrinePanel/DoctrineProgress").Text.Contains("TANK DOCTRINE CHOSEN")).IsTrue();
	}
}
