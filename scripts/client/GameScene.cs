using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GameScene : Node2D
{
	[Signal]
	public delegate void UnitSelectionEventHandler();
	public bool AbilityTargetSelection;
	private Vector2? AbilityTargetPosition = null;
	private string lastSelectedAbility = "";
	private TimeTickSystem gameLoop = null;
	private LocalSimulationNode simulationCore = null;
	private readonly PackedScene ConstructionSitePreviewScene = GD.Load<PackedScene>("res://scenes/buildings/ConstructionSite.tscn");
	private Node2D constructionPreview = null;
	private HashSet<string> EntitySelectionIds = new();
	public IReadOnlyCollection<string> SelectedEntityIds => EntitySelectionIds;
	private bool shiftHeld = false;
	/*
	Selection Box
	*/
	private bool IsDraggingSelection = false;
	private Vector2 SelectionStartPos;
	private Vector2 SelectionEndPos;
	private bool IsPotentialSelectionDrag = false;
	private const float DragThreshold = 6f;
	public override void _Ready()
	{
		gameLoop = GetNode<TimeTickSystem>("GameLoop");
		simulationCore = GetNode<LocalSimulationNode>("SimulationCore");
		if (simulationCore is null)
		{
			GD.PushError("SimulationCore node was not found or has the wrong script");
			return;
		}
		simulationCore.StateChanged += PruneDeletedSelectedEntities;

		SelectedEntityUI selectedEntityUI = GetNode<SelectedEntityUI>("Camera2D/Screen/GameUserInterface/MainLayout/VBoxContainer/BottomBar/SelectedEntityUi");
		selectedEntityUI.AbilityPressed += OnAbilityPressed;


		// Debuging 
		// Spawn units in order to test the game
		simulationCore.Push(new DebugSpawnUnitsMessage(
			"player_1",
			gameLoop.CurrentTick,
			UnitType.CONSTRUCTION_UNIT,
			new Vector2(100, 50),
			UnitCatalog.GetMovementSpeed(UnitType.CONSTRUCTION_UNIT)
		));

		for (int i = 0; i < 3; i++)
		{
			simulationCore.Push(new DebugSpawnUnitsMessage(
				"player_2",
				gameLoop.CurrentTick,
				UnitType.BASIC_INFANTRY,
				new Vector2(150 + i * 50, 100),
				UnitCatalog.GetMovementSpeed(UnitType.BASIC_INFANTRY)
			));
		}
	}

	public override void _Process(double delta)
	{
		if (constructionPreview is not null)
			constructionPreview.GlobalPosition = GetGlobalMousePosition();
	}

	private void PruneDeletedSelectedEntities()
	{
		if (EntitySelectionIds.Count <= 0)
			return;

		var existingEntityIds = simulationCore.GetState()
			.Players.Values
			.SelectMany(player => player.Entities.Keys)
			.ToHashSet();

		var removedSelection = EntitySelectionIds.RemoveWhere(entityId => !existingEntityIds.Contains(entityId));
		if (removedSelection <= 0)
			return;

		EmitSignal(SignalName.UnitSelection);
	}

	private void OnAbilityPressed(string abilityId)
	{
		if (AbilityCatalog.RequiresTarget(abilityId) && AbilityTargetPosition is null)
		{
			lastSelectedAbility = abilityId;
			AbilityTargetSelection = true;
			ShowConstructionPreview(abilityId);
			return;
		}

		var command = new UseAbilityMessage(
			"player_1",
			gameLoop.CurrentTick,
			SelectedEntityIds.ToArray(),
			abilityId,
			shiftHeld ? 1 : 0,
			(AbilityTargetPosition is null) ? null : AbilityTargetPosition
		);

		AbilityTargetPosition = null;
		AbilityTargetSelection = false;
		lastSelectedAbility = "";
		ClearConstructionPreview();

		if (!simulationCore.Push(command))
		{
			GD.Print("Ability command coulnd't be processed.");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (GetViewport().GuiGetHoveredControl() is BaseButton)
				return;

			shiftHeld = Input.IsKeyPressed(Key.Shift);

			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				SelectionStartPos = GetGlobalMousePosition();
				SelectionEndPos = GetGlobalMousePosition();
				IsPotentialSelectionDrag = true;
				IsDraggingSelection = false;
			}

			if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
			{
				SelectionEndPos = GetGlobalMousePosition();

				if (IsDraggingSelection)
				{
					HandleSelectionBox(SelectionStartPos, SelectionEndPos);
				}
				else
				{
					HandleLeftMouseButton(shiftHeld);
				}

				IsPotentialSelectionDrag = false;
				IsDraggingSelection = false;
				QueueRedraw();
			}

			if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				if (AbilityTargetSelection)
				{
					ClearAbilityTargetSelection();
					return;
				}

				HandleRightMouseButton(shiftHeld);
			}
		}

		if (@event is InputEventMouseMotion && IsPotentialSelectionDrag)
		{
			SelectionEndPos = GetGlobalMousePosition();

			if (Input.IsMouseButtonPressed(MouseButton.Left) &&
				SelectionStartPos.DistanceTo(SelectionEndPos) >= DragThreshold)
			{
				IsDraggingSelection = true;
				QueueRedraw();
			}
		}
	}

	public override void _Draw()
	{
		if (!IsDraggingSelection)
			return;

		var rect = new Rect2(
			SelectionStartPos,
			SelectionEndPos - SelectionStartPos
		).Abs();

		DrawRect(rect, new Color(0.2f, 0.9f, 0.0f, 0.8f), false, 2f);
	}


	private void HandleSelectionBox(Vector2 start, Vector2 end)
	{
		var rect = new Rect2(
			start,
			end - start
		).Abs();

		if (!shiftHeld)
			EntitySelectionIds.Clear();

		var worldRenderer = GetNode<ClientWorldRenderer>("WorldRenderer");

		foreach (var child in worldRenderer.GetChildren())
		{
			if (child is not ClientUnit unit)
				continue;


			if (unit.OwnerPlayerId != "player_1")
				continue;

			if (rect.HasPoint(unit.GlobalPosition))
			{
				EntitySelectionIds.Add(unit.EntityId);
			}
		}

		EmitSignal(SignalName.UnitSelection);
	}

	private void ShowConstructionPreview(string abilityId)
	{
		ClearConstructionPreview();

		if (abilityId != "spawn_barracks")
			return;

		constructionPreview = ConstructionSitePreviewScene.Instantiate<Node2D>();
		constructionPreview.Modulate = new Color(1f, 1f, 1f, 0.45f);
		constructionPreview.ZIndex = 100;
		DisablePreviewPicking(constructionPreview);
		AddChild(constructionPreview);
		constructionPreview.GlobalPosition = GetGlobalMousePosition();
	}

	private void ClearAbilityTargetSelection()
	{
		AbilityTargetPosition = null;
		AbilityTargetSelection = false;
		lastSelectedAbility = "";
		ClearConstructionPreview();
	}

	private void ClearConstructionPreview()
	{
		if (constructionPreview is null)
			return;

		constructionPreview.QueueFree();
		constructionPreview = null;
	}

	private static void DisablePreviewPicking(Node node)
	{
		if (node is Area2D area)
		{
			area.Monitoring = false;
			area.Monitorable = false;
			area.CollisionLayer = 0;
			area.CollisionMask = 0;
		}
		else if (node is CollisionShape2D collisionShape)
		{
			collisionShape.Disabled = true;
		}

		foreach (var child in node.GetChildren())
			DisablePreviewPicking(child);
	}

	private void HandleLeftMouseButton(bool shiftHeld)
	{
		if (AbilityTargetSelection)
		{
			AbilityTargetPosition = GetGlobalMousePosition();
			OnAbilityPressed(lastSelectedAbility);
			return;
		}

		var CurrentEntity = GetEntityUnderMouse(GetGlobalMousePosition());
		if (CurrentEntity is null)
		{
			EntitySelectionIds.Clear();
			EmitSignal(SignalName.UnitSelection);
			return;
		}

		if (CurrentEntity is ClientBuilding building)
		{
			EntitySelectionIds.Clear();
		}

		if (!shiftHeld)
		{
			EntitySelectionIds.Clear();
		}
		EntitySelectionIds.Add(CurrentEntity.EntityId);
		EmitSignal(SignalName.UnitSelection);
	}

	private void HandleRightMouseButton(bool shiftHeld)
	{
		MessageBase command = null;

		var CurrentEntity = GetEntityUnderMouse(GetGlobalMousePosition());
		if (CurrentEntity is null)
		{
			if (SelectedEntityIds.Count <= 0)
				return;

			command = new MoveUnitsMessage(
				"player_1",
				gameLoop.CurrentTick,
				SelectedEntityIds.ToArray<string>(),
				GetGlobalMousePosition(),
				shiftHeld ? 1 : 0,
				"rectangle"
			);
		}
		else if (CurrentEntity.OwnerPlayerId != "player_1")
		{
			if (SelectedEntityIds.Count <= 0)
				return;

			command = new AttackTargetMessage(
				"player_1",
				gameLoop.CurrentTick,
				SelectedEntityIds.ToArray(),
				CurrentEntity.EntityId,
				shiftHeld ? 1 : 0
			);
		}
		else if (CurrentEntity is ClientBuilding { Type: BuildingType.CONSTRUCTION_SITE })
		{
			if (SelectedEntityIds.Count <= 0)
				return;

			command = new RepairTargetMessage(
				"player_1",
				gameLoop.CurrentTick,
				SelectedEntityIds.ToArray(),
				CurrentEntity.EntityId,
				shiftHeld ? 1 : 0
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
