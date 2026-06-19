using Godot;
using System;

public partial class TestScene : Node2D
{
	private TimeTickSystem gameLoop = null!;
	private LocalSimulationNode simulationCore = null!;
	private bool SpawnUnits = false;
	public override void _Ready()
	{
		gameLoop = GetNode<TimeTickSystem>("GameLoop");
		simulationCore = GetNode<LocalSimulationNode>("SimulationCore");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (simulationCore is null)
			{
				GD.PushError("SimulationCore node was not found or has the wrong script");
				return;
			}

			if (keyEvent.Keycode == Key.Space)
			{
				SpawnUnits = !SpawnUnits;
			}
		}
		
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (simulationCore is null)
			{
				GD.PushError("SimulationCore node was not found or has the wrong script");
				return;
			}

			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				HandleLeftMouseButton();
			}

			else if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				HandleRightMouseButton();
			}
		}
	}

	private void HandleLeftMouseButton()
	{
		MessageBase command = null;
		if (!SpawnUnits) {
			command = new BuildStructureMessage(
				//TODO:
				BuildingType.BASIC_GENERATOR,
				"player_1",
				gameLoop.CurrentTick,
				"worker_1",
				GetGlobalMousePosition()
			);
		} else
		{
			command = new DebugSpawnUnitsMessage(
				"player_1",
				gameLoop.CurrentTick,
				//TODO:
				UnitType.BASIC_INFANTRY,
				GetGlobalMousePosition()		
			);
		}

		if (!simulationCore.Push(command))
			GD.Print("Command couldn't be processed. ", command);

		GD.Print("Command send");
	}

	private void HandleRightMouseButton()
	{
		var CurrentBuilding = GetBuildingUnderMouse(GetGlobalMousePosition());
		if (CurrentBuilding is null) {
			GD.Print("No buiding was clicked");
			return;
		}

		if (CurrentBuilding.BuildProgression >= 100)
		{
			GD.Print("Cannot cancel finished Building");
			return;
		}

		var command = new CancelConstructionMessage(
			"player_1",
			gameLoop.CurrentTick,
			CurrentBuilding.EntityId
		);

		if (!simulationCore.Push(command))
			GD.Print("Command couldn't be processed. ");

		GD.Print("Command send");
	}
	private TestBuilding? GetBuildingUnderMouse(Vector2 worldPosition)
	{
		var query = new PhysicsPointQueryParameters2D
		{
			Position = worldPosition,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var results = GetWorld2D().DirectSpaceState.IntersectPoint(query);

		foreach (var result in results)
		{
			if (result["collider"].AsGodotObject() is Area2D area)
			{
				var building = area.GetParent() as TestBuilding;

				if (building is not null)
					return building;
			}
		}

		return null;
	}
}
