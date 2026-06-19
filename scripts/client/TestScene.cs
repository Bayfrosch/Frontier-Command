using Godot;
using System;

public partial class TestScene : Node
{
	[Export] public PackedScene BuildingScene { get; set; } = null!;
	private TimeTickSystem gameLoop = null!;
	private LocalSimulationNode simulationCore = null!;
	public override void _Ready()
	{
		gameLoop = GetNode<TimeTickSystem>("GameLoop");
		simulationCore = GetNode<LocalSimulationNode>("SimulationCore");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left && 
			mouseEvent.Pressed)
		{
			if (simulationCore is null)
			{
				GD.PushError("SimulationCore node was not found or has the wrong script");
				return;
			}
			var command = new BuildStructureMessage(
				BuildingType.BASIC_GENERATOR,
				"player_1",
				gameLoop.CurrentTick,
				"worker_1",
				mouseEvent.Position
			);

			if (!simulationCore.Push(command))
				GD.Print("Command couldn't be processed. ", command);
			
			GD.Print("Command send");
		}
	}
}
