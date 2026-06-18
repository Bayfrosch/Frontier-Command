using Godot;
using System;

public partial class test_obj : Node
{
	public override void _Ready()
	{
		TimeTickSystem game_loop = GetNode<TimeTickSystem>("../GameLoop");
		game_loop.Tick += OnTick;
	}

	private void OnTick(int tick)
	{
		GD.Print($"Received tick {tick}");
	}
}
