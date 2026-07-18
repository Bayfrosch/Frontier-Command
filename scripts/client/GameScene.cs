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
	private bool lastSelectedAbilityRequiresTarget = false;
	private TimeTickSystem gameLoop = null;
	private LocalSimulationNode simulationCore = null;
	private readonly PackedScene ConstructionSitePreviewScene = GD.Load<PackedScene>("res://scenes/buildings/ConstructionSite.tscn");
	private Label materialCount = null;
	private EnergyBar energyBar = null;
	private static readonly Dictionary<string, BuildingType> ConstructionPreviewTypes = new()
	{
		["spawn_barracks"] = BuildingType.BARRACKS,
		["spawn_power_plant"] = BuildingType.POWER_PLANT,
		["spawn_resource_gatherer"] = BuildingType.RESOURCE_GATHERER,
		["spawn_war_factory"] = BuildingType.WAR_FACTORY
	};
	private static readonly Color ValidConstructionPreviewColor = new(1f, 1f, 1f, 0.45f);
	private static readonly Color InvalidConstructionPreviewColor = new(1f, 0.15f, 0.15f, 0.65f);
	private Node2D constructionPreview = null;
	private BuildingType? constructionPreviewBuildingType = null;
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

		SelectedEntityUI selectedEntityUI = GetNodeOrNull<SelectedEntityUI>("Camera2D/Screen/GameUserInterface/MainLayout/VBoxContainer/BottomBar/VBoxContainer/SelectedEntityUi");
		if (selectedEntityUI is null)
		{
			GD.PushError("SelectedEntityUi node was not found or has the wrong script");
			return;
		}
		selectedEntityUI.AbilityPressed += OnAbilityPressed;

		materialCount = GetNode<Label>(
			"Camera2D/Screen/GameUserInterface/MainLayout/VBoxContainer/TopBar/MaterialCount"
		);
		energyBar = GetNode<EnergyBar>(
			"Camera2D/Screen/GameUserInterface/MainLayout/VBoxContainer/TopBar/EnergyBar"
		);

		simulationCore.StateChanged += UpdateResourceUi;
		UpdateResourceUi();

		// Debuging 
		// Spawn Command Center
		simulationCore.Push(new DebugSpawnBuildingMessage(
			"player_1",
			gameLoop.CurrentTick,
			BuildingType.COMMAND_CENTER,
			new Vector2(100, 100)
		));

		// Spawn units in order to test the game
		simulationCore.Push(new DebugSpawnUnitsMessage(
			"player_1",
			gameLoop.CurrentTick,
			UnitType.CONSTRUCTION_UNIT,
			new Vector2(100, 250),
			UnitCatalog.GetMovementSpeed(UnitType.CONSTRUCTION_UNIT)
		));

		for (int i = 0; i < 5; i++)
		{
			simulationCore.Push(new DebugSpawnUnitsMessage(
				"player_2",
				gameLoop.CurrentTick,
				UnitType.BASIC_INFANTRY,
				new Vector2(-500, 0 + i * 50),
				UnitCatalog.GetMovementSpeed(UnitType.BASIC_INFANTRY)
			));
		}
		// Sawn resources
		simulationCore.Push(new DebugSpawnBuildingMessage(
			"neutral",
			gameLoop.CurrentTick,
			BuildingType.RESOURCE_SPAWNER,
			new Vector2(800, 100)
		));
	}

	public override void _Process(double delta)
	{
		if (constructionPreview is null || constructionPreviewBuildingType is null)
			return;

		constructionPreview.GlobalPosition = GetGlobalMousePosition();
		UpdateConstructionPreviewAppearance();
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

	private void OnAbilityPressed(string abilityId, bool requiresTarget)
	{
		if (!IsAbilityUnlockedForLocalPlayer(abilityId))
		{
			ClearAbilityTargetSelection();
			return;
		}

		if (requiresTarget && AbilityTargetPosition is null)
		{
			lastSelectedAbility = abilityId;
			lastSelectedAbilityRequiresTarget = requiresTarget;
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
		lastSelectedAbilityRequiresTarget = false;
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
			if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
			{
				if (!IsPotentialSelectionDrag && !IsDraggingSelection)
					return;

				SelectionEndPos = GetGlobalMousePosition();

				if (IsDraggingSelection)
				{
					HandleSelectionBox(SelectionStartPos, SelectionEndPos);
					ClearSelectionDragState();
					GetViewport().SetInputAsHandled();
					return;
				}

				if (GetViewport().GuiGetHoveredControl() is not null)
				{
					ClearSelectionDragState();
					return;
				}
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

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseEvent)
			return;

		shiftHeld = Input.IsKeyPressed(Key.Shift);

		if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
		{
			SelectionStartPos = GetGlobalMousePosition();
			SelectionEndPos = GetGlobalMousePosition();
			IsPotentialSelectionDrag = true;
			IsDraggingSelection = false;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
		{
			if (!IsPotentialSelectionDrag)
				return;

			SelectionEndPos = GetGlobalMousePosition();

			if (IsDraggingSelection)
			{
				HandleSelectionBox(SelectionStartPos, SelectionEndPos);
			}
			else
			{
				HandleLeftMouseButton(shiftHeld);
			}

			ClearSelectionDragState();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
		{
			if (AbilityTargetSelection)
			{
				ClearAbilityTargetSelection();
				GetViewport().SetInputAsHandled();
				return;
			}

			HandleRightMouseButton(shiftHeld);
			GetViewport().SetInputAsHandled();
		}
	}

	private void ClearSelectionDragState()
	{
		IsPotentialSelectionDrag = false;
		IsDraggingSelection = false;
		QueueRedraw();
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

		if (!IsAbilityUnlockedForLocalPlayer(abilityId))
			return;

		if (!ConstructionPreviewTypes.TryGetValue(abilityId, out var previewBuildingType))
			return;

		constructionPreview = ConstructionSitePreviewScene.Instantiate<Node2D>();
		constructionPreviewBuildingType = previewBuildingType;
		ConfigureConstructionPreviewFootprint(constructionPreview, previewBuildingType);
		constructionPreview.ZIndex = 100;
		ClientWorldInput.IgnoreGuiMouse(constructionPreview);
		DisablePreviewPicking(constructionPreview);
		AddChild(constructionPreview);
		constructionPreview.GlobalPosition = GetGlobalMousePosition();
		UpdateConstructionPreviewAppearance();
	}

	private void UpdateConstructionPreviewAppearance()
	{
		if (constructionPreview is null || constructionPreviewBuildingType is null)
			return;

		constructionPreview.Modulate = IsConstructionPlacementValid(
			constructionPreviewBuildingType.Value,
			constructionPreview.GlobalPosition)
			? ValidConstructionPreviewColor
			: InvalidConstructionPreviewColor;
	}

	private bool IsConstructionPlacementValid(BuildingType buildingType, Vector2 position)
	{
		return !simulationCore.GetState()
			.Players.Values
			.SelectMany(player => player.Entities.Values)
			.OfType<BuildingState>()
			.Where(building => building.Health > 0)
			.Any(building => BuildingCatalog.FootprintsOverlap(
				buildingType,
				position,
				BuildingCatalog.GetFootprintType(building),
				building.CurrentPosition));
	}

	private static void ConfigureConstructionPreviewFootprint(Node2D preview, BuildingType buildingType)
	{
		var footprintSize = BuildingCatalog.GetFoodprintSize(buildingType);

		if (preview.GetNodeOrNull<ColorRect>("BodyRender") is ColorRect body)
		{
			body.OffsetLeft = -footprintSize.X / 2f;
			body.OffsetTop = -footprintSize.Y / 2f;
			body.OffsetRight = footprintSize.X / 2f;
			body.OffsetBottom = footprintSize.Y / 2f;
		}

		if (preview.GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D") is CollisionShape2D collisionShape
			&& collisionShape.Shape is RectangleShape2D rectangleShape)
		{
			var previewShape = rectangleShape.Duplicate() as RectangleShape2D;
			if (previewShape is null)
				return;

			previewShape.Size = footprintSize;
			collisionShape.Shape = previewShape;
		}
	}

	private void ClearAbilityTargetSelection()
	{
		AbilityTargetPosition = null;
		AbilityTargetSelection = false;
		lastSelectedAbility = "";
		lastSelectedAbilityRequiresTarget = false;
		ClearConstructionPreview();
	}

	private void ClearConstructionPreview()
	{
		constructionPreviewBuildingType = null;

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
			var targetPosition = GetGlobalMousePosition();
			if (constructionPreviewBuildingType is BuildingType buildingType
				&& !IsConstructionPlacementValid(buildingType, targetPosition))
			{
				ClearAbilityTargetSelection();
				return;
			}

			AbilityTargetPosition = targetPosition;
			OnAbilityPressed(lastSelectedAbility, lastSelectedAbilityRequiresTarget);
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
		else if (CurrentEntity is ClientResource)
		{
			if (SelectedEntityIds.Count <= 0)
				return;

			command = new GatherResourcesMessage(
				"player_1",
				gameLoop.CurrentTick,
				SelectedEntityIds.ToArray(),
				CurrentEntity.EntityId,
				shiftHeld ? 1 : 0
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
		else if (CurrentEntity is ClientBuilding building)
		{
			if (building is ConstructionSite)
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

	private void UpdateResourceUi()
	{
		if (!simulationCore.GetState().Players.TryGetValue("player_1", out var player))
			return;

		materialCount.Text = $"Virelium: {player.Virelium}";
		energyBar.ApplyEnergy(player.EnergyProduced, player.EnergyConsumed);
	}

	private bool IsAbilityUnlockedForLocalPlayer(string abilityId)
	{
		if (!simulationCore.GetState().Players.TryGetValue("player_1", out var player))
			return false;

		return player.UnlockedAbilities.Contains(abilityId);
	}

	private ClientEntity? ExtractClientEntity(Godot.Collections.Dictionary result)
	{
		if (result["collider"].AsGodotObject() is not Area2D area)
			return null;

		return area.GetParent() as ClientEntity;
	}
}
