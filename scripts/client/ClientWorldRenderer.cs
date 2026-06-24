using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientWorldRenderer : Node
{
	// Building Scenes
	public PackedScene TestBuildingScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/building.tscn");
	public PackedScene BasicGeneratorScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/building.tscn");

	// Unit Scenes
	public PackedScene BasicInfantryScene { get; } = GD.Load<PackedScene>("res://scenes/units/basicInfantry.tscn");

	private static readonly Color DefaultUnitColor = new(0.7169325f, 0.3354848f, 0.33498362f, 1f);
	private static readonly Color SelectedUnitColor = new(0.2f, 0.8f, 1f, 1f);
	private LocalSimulationNode simulation = null;
	private GameScene Scene = null;
	private readonly Dictionary<string, ClientBuilding> buildingsById = new();
	private readonly Dictionary<string, ClientUnit> unitsById = new();

	public override void _Ready()
	{
		simulation = GetNode<LocalSimulationNode>("../SimulationCore");
		if (simulation is null)
		{
			GD.Print("LocalSimulationNode not found");
			return;
		}

		simulation.StateChanged += SyncFromState;

		Scene = GetParent<GameScene>();
		if (Scene is null)
		{
			GD.Print("Scene not found");
			return;
		}

		Scene.UnitSelection += OnUnitSelection;
	}

	private void OnUnitSelection()
	{
		foreach (var unit in unitsById.Values)
		{
			var bodyRender = unit.GetNode<ColorRect>("BodyRender");
			bodyRender.Color = Scene.SelectedEntityIds.Contains(unit.EntityId)
				? SelectedUnitColor
				: DefaultUnitColor;
		}
	}

	/*
	Synchronizes simulated EntityStates with rendered entities
	Spawns and removes buildings according to the given list of existing entities
	Cannot change actual values, can only render
	*/
	private void SyncFromState()
	{
		var state = simulation.GetState();
		var existingBuildingIds = new HashSet<string>();
		var existingUnitIds = new HashSet<string>();

		// Build
		foreach (var player in state.Players.Values)
		{
			foreach (var entity in player.Entities.Values)
			{
				if (entity is BuildingState buildingState)
				{
					existingBuildingIds.Add(buildingState.EntityId);
					SyncBuilding(buildingState);
				} else if (entity is UnitState unitState)
				{
					existingUnitIds.Add(unitState.EntityId);
					SyncUnit(unitState);
				}	
			}
		}

		// Remove
		foreach (var entityId in buildingsById.Keys.ToArray())
		{
			if (!existingBuildingIds.Contains(entityId))
			{
				buildingsById[entityId].QueueFree();
				buildingsById.Remove(entityId);
			}
		}
	}

	private PackedScene GetBuildingScene(BuildingType type)
	{
		return type switch
		{
			BuildingType.BASIC_GENERATOR => BasicGeneratorScene,
			_ => TestBuildingScene
		};
	}

	private PackedScene GetUnitScene(UnitType type)
	{
		return type switch
		{
			UnitType.BASIC_INFANTRY => BasicInfantryScene,
			_ => BasicInfantryScene
		};
	}

	private void SyncBuilding(BuildingState buildingState)
	{
		if (!buildingsById.TryGetValue(buildingState.EntityId, out var building))
		{
			var scene = GetBuildingScene(buildingState.Type);
			building = scene.Instantiate<ClientBuilding>();
			AddChild(building);
			buildingsById[buildingState.EntityId] = building;

			building.ApplySpawnState(buildingState);
			return;
		}

		building.ApplyState(buildingState);
	}

	private void SyncUnit(UnitState unitState)
	{
		if (!unitsById.TryGetValue(unitState.EntityId, out var unit))
		{
			var scene = GetUnitScene(unitState.Type);
			unit = scene.Instantiate<ClientUnit>();
			AddChild(unit);
			unitsById[unitState.EntityId] = unit;

			unit.ApplySpawnState(unitState);
			return;
		}

		unit.ApplyState(unitState);
	}
}
