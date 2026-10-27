using Godot;
using GdUnit4;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ClientWorldRendererTest
{
	[TestCase(UnitType.BASIC_INFANTRY)]
	[TestCase(UnitType.RPG_TROOPER)]
	[TestCase(UnitType.LIGHT_TANK)]
	[TestCase(UnitType.CONSTRUCTION_UNIT)]
	[TestCase(UnitType.RESOURCE_COLLECTOR)]
	public async Task SelectingAndDeselectingRestoresUnitAppearance(UnitType type)
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = (GameScene)runner.Scene();
		scene.GetNode<TimeTickSystem>("GameLoop").SetProcess(false);
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var renderer = scene.GetNode<ClientWorldRenderer>("WorldRenderer");
		var position = new Vector2(600, 600);
		AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
			"player_1", 0, type, position, position, 100f))).IsTrue();
		await runner.SimulateFrames(2);
		var unit = renderer.GetChildren().OfType<ClientUnit>().Single(unit => unit.GlobalPosition == position);
		var body = unit.GetNodeOrNull<ColorRect>("BodyRender");
		var sprite = unit.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		var originalColor = body is not null ? body.Color : sprite.Modulate;

		scene.Call("SelectAtWorldPosition", position, false);
		AssertThat(scene.SelectedEntityIds.Contains(unit.EntityId)).IsTrue();
		AssertThat(body is not null ? body.Color : sprite.Modulate).IsEqual(new Color(0.2f, 0.8f, 1f));
		AssertThat(unit.GetNode<ProgressBar>("HealthBar").Modulate).IsEqual(Colors.White);

		scene.Call("SelectAtWorldPosition", new Vector2(2000, 2000), false);
		AssertThat(scene.SelectedEntityIds).IsEmpty();
		AssertThat((body is not null ? body.Color : sprite.Modulate).ToHtml()).IsEqual(originalColor.ToHtml());
	}

	[TestCase]
	public async Task SquadIconSpawnsAboveCenterAndMovesSmoothly()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		scene.GetNode<TimeTickSystem>("GameLoop").SetProcess(false);
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var renderer = scene.GetNode<ClientWorldRenderer>("WorldRenderer");
		var position = new Vector2(600, 600);
		var iconOffset = new Vector2(0, -80);

		AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
			"player_1", 0, UnitType.BASIC_INFANTRY, position, position, 100f, "squad-1"))).IsTrue();
		AssertThat(renderer.GetChildren().OfType<ClientUnit>()
			.Count(unit => unit.SquadId == "squad-1")).IsEqual(1);
		var icon = renderer.GetChildren().OfType<ClientSquadState>().Single();
		AssertThat(icon.SquadId).IsEqual("squad-1");
		AssertThat(icon.GlobalPosition).IsEqual(position + iconOffset);

		var secondPosition = new Vector2(640, 600);
		AssertThat(simulation.Push(new DebugSpawnUnitsMessage(
			"player_1", 0, UnitType.BASIC_INFANTRY, secondPosition, secondPosition, 100f, "squad-1"))).IsTrue();
		AssertThat(renderer.GetChildren().OfType<ClientSquadState>().Count()).IsEqual(1);
		AssertThat(icon.GlobalPosition).IsEqual(position + iconOffset);
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(610, 600) + iconOffset);
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(620, 600) + iconOffset);

		var player = simulation.GetState().Players["player_1"];
		foreach (var unit in player.Squads["squad-1"].Soldiers.Values)
		{
			unit.SetMoveOrder(unit.CurrentPosition + new Vector2(20, 0));
			unit.AdvanceMovement(0.2f);
		}
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(620, 600) + iconOffset);
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(630, 600) + iconOffset);
		icon._Process(0.1);
		AssertThat(icon.GlobalPosition).IsEqual(new Vector2(640, 600) + iconOffset);

		foreach (var unitId in player.Squads["squad-1"].Soldiers.Keys.ToArray())
			player.RemoveEntity(unitId);
		simulation.EmitSignal(LocalSimulationNode.SignalName.StateChanged);
		await runner.SimulateFrames(2);
		AssertThat(renderer.GetChildren().OfType<ClientSquadState>().Count()).IsEqual(0);
	}
}
