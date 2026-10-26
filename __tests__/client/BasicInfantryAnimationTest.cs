using Godot;
using GdUnit4;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BasicInfantryAnimationTest
{
	[TestCase(0, 100, "front", false)]
	[TestCase(0, -100, "back", false)]
	[TestCase(100, 0, "right", false)]
	[TestCase(-100, 0, "right", true)]
	[TestCase(-100, -25, "right", true)]
	[TestCase(-25, -100, "back", false)]
	public void MovementAnimatesAndStoppingPlaysIdle(int x, int y, string animation, bool flipH)
	{
		using var runner = ISceneRunner.Load("res://scenes/units/basicInfantry.tscn", true, false);
		var unit = (BasicInfantry)runner.Scene();
		unit.SetProcess(false);
		var sprite = unit.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		var state = new UnitState("infantry", "player_1", Vector2.Zero, 100f, UnitType.BASIC_INFANTRY);
		unit.ApplySpawnState(state);
		unit._Process(0.1);
		AssertThat(sprite.IsPlaying()).IsTrue();
		AssertThat(sprite.Animation.ToString()).IsEqual("idle");

		state.SetMoveOrder(new Vector2(x, y));
		state.AdvanceMovement(0.5f);
		unit.ApplyState(state);
		unit._Process(0.1);
		AssertThat(sprite.IsPlaying()).IsTrue();
		AssertThat(sprite.Animation.ToString()).IsEqual(animation);
		AssertThat(sprite.FlipH).IsEqual(flipH);

		for (var frame = 0; frame < 6; frame++)
			unit._Process(0.1);
		AssertThat(sprite.IsPlaying()).IsTrue();
		AssertThat(sprite.Animation.ToString()).IsEqual("idle");
		AssertThat(sprite.FlipH).IsFalse();
		AssertThat(sprite.Frame).IsEqual(0);
		AssertThat(unit.GlobalPosition).IsEqual(state.CurrentPosition);
	}

	[TestCase]
	public async Task FinalMovementPlaysFramesAndTurningClearsMirroring()
	{
		using var runner = ISceneRunner.Load("res://scenes/units/basicInfantry.tscn", true, false);
		var unit = (BasicInfantry)runner.Scene();
		unit.SetProcess(false);
		var sprite = unit.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		var state = new UnitState("infantry", "player_1", Vector2.Zero, 100f, UnitType.BASIC_INFANTRY);
		unit.ApplySpawnState(state);
		state.SetMoveOrder(new Vector2(-20, 0));
		state.AdvanceMovement(0.5f);
		unit.ApplyState(state);
		AssertThat(unit.HasMoveOrder).IsFalse();

		unit._Process(0.1);
		AssertThat(sprite.IsPlaying()).IsTrue();
		AssertThat(sprite.FlipH).IsTrue();
		await runner.SimulateFrames(10, 20);
		AssertThat(sprite.Frame).IsGreater(0);
		unit._Process(0.1);
		unit._Process(0.1);
		AssertThat(sprite.IsPlaying()).IsTrue();
		AssertThat(sprite.Animation.ToString()).IsEqual("idle");
		AssertThat(sprite.FlipH).IsFalse();
		for (var frame = 0; frame < 15; frame++)
		{
			unit._Process(0.02);
			await runner.SimulateFrames(1, 20);
		}
		AssertThat(sprite.Frame).IsGreater(0);

		state.SetMoveOrder(state.CurrentPosition + new Vector2(0, 100));
		state.AdvanceMovement(0.5f);
		unit.ApplyState(state);
		unit._Process(0.1);
		AssertThat(sprite.Animation.ToString()).IsEqual("front");
		AssertThat(sprite.FlipH).IsFalse();
	}
}
