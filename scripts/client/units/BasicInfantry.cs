using Godot;

public partial class BasicInfantry : ClientUnit
{
	private AnimatedSprite2D Sprite;

	public override void _Ready()
	{
		base._Ready();
		Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Sprite.Play("idle");
	}

	public override void _Process(double delta)
	{
		var previousPosition = GlobalPosition;
		base._Process(delta);
		var movement = GlobalPosition - previousPosition;
		if (movement.IsZeroApprox())
		{
			Sprite.FlipH = false;
			Sprite.Play("idle");
			return;
		}

		var movingSideways = Mathf.Abs(movement.X) > Mathf.Abs(movement.Y);
		Sprite.FlipH = movingSideways && movement.X < 0f;
		Sprite.Play(movingSideways ? "right" : movement.Y < 0f ? "back" : "front");
	}
}
