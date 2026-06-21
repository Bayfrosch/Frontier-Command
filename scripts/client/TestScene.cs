using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TestScene : Node2D
{
	[Signal] 
	public delegate void UnitSelectionEventHandler();
	private TimeTickSystem gameLoop = null;
	private LocalSimulationNode simulationCore = null;
	private bool SpawnUnits = false;
	private bool SpawnMode = false;
	private HashSet<string> UnitSelectionIds = new();
	public IReadOnlyCollection<string> SelectedUnitIds => UnitSelectionIds;
	public override void _Ready()
	{
		gameLoop = GetNode<TimeTickSystem>("GameLoop");
		simulationCore = GetNode<LocalSimulationNode>("SimulationCore");
		if (simulationCore is null)
		{
			GD.PushError("SimulationCore node was not found or has the wrong script");
			return;
		}
	}

	private void _on_spawn_button_pressed()
	{
		SpawnMode = !SpawnMode;
		GD.Print($"Spawn Mode: {SpawnMode}");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Space)
			{
				SpawnUnits = !SpawnUnits;
			}
		}
		
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (GetViewport().GuiGetHoveredControl() is BaseButton) 
				return;
			
			bool leftClicked = mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed;
			bool shiftHeld = Input.IsKeyPressed(Key.Shift);
			if (leftClicked)
			{
				HandleLeftMouseButton(shiftHeld);
			}

			else if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				HandleRightMouseButton(shiftHeld);
			}
		}
	}

	private void HandleLeftMouseButton(bool shiftHeld)
	{
		if (SpawnMode) {
			MessageBase command;
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
					GetGlobalMousePosition(),
					25f	
				);
			}

			if (!simulationCore.Push(command))
				GD.Print("Command couldn't be processed. ", command);

			GD.Print("Command send");
		} else
		{
			var CurrentEntity = GetEntityUnderMouse(GetGlobalMousePosition());
			if (CurrentEntity is null || CurrentEntity is ClientBuilding)
			{
				UnitSelectionIds.Clear();
				EmitSignal(SignalName.UnitSelection);
				return;
			}

			if (CurrentEntity is not ClientUnit unit)
			{
				return;
			}

			if (!shiftHeld)
			{
				UnitSelectionIds.Clear();
			}
			UnitSelectionIds.Add(unit.EntityId);
			EmitSignal(SignalName.UnitSelection);
		}
	}

	private void HandleRightMouseButton(bool shiftHeld)
	{
		MessageBase command = null;

		var CurrentBuilding = GetEntityUnderMouse(GetGlobalMousePosition());
		if (!(CurrentBuilding is null)) {
			if (CurrentBuilding is not ClientBuilding building)
			{
				return;
			}

			if (building.BuildProgression >= 100)
			{
				GD.Print("Cannot cancel finished Building");
				return;
			}

			command = new CancelConstructionMessage(
				"player_1",
				gameLoop.CurrentTick,
				building.EntityId
			);
		} else
		{
			command = new MoveUnitsMessage(
				"player_1",
				gameLoop.CurrentTick,
				SelectedUnitIds.ToArray<string>(),
				GetGlobalMousePosition(),
				shiftHeld ? 1 : 0,
				"rectangle"
			);
		}

		if (command is null) 
			return;
		
		if (!simulationCore.Push(command))
			GD.Print("Command couldn't be processed. ");

		GD.Print("Command send");
	}
	private ClientEntity? GetEntityUnderMouse(Vector2 worldPosition)
	{
		var query = new PhysicsPointQueryParameters2D
		{
			Position = worldPosition,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var results = GetWorld2D().DirectSpaceState.IntersectPoint(query);

		return results
			.Select(result => ExtractClientEntity(result))
			.Where(entity => entity is not null)
			.OrderBy(entity => GetSelectionPriority(entity))
			.FirstOrDefault();
	}

	private int GetSelectionPriority(ClientEntity entity)
	{
		bool isOwned = entity.OwnerPlayerId == "player_1";

		if (entity is ClientUnit && isOwned)
			return 0;

		if (entity is ClientBuilding && isOwned)
			return 1;

		if (entity is ClientUnit)
			return 2;

		if (entity is ClientBuilding)
			return 3;

		return 4;
	}

	private ClientEntity? ExtractClientEntity(Godot.Collections.Dictionary result)
	{
		if (result["collider"].AsGodotObject() is not Area2D area)
			return null;

		return area.GetParent() as ClientEntity;
	}
}
