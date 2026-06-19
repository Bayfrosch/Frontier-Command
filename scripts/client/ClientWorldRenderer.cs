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

	private LocalSimulationNode simulation = null!;
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
		}

		unit.ApplyState(unitState);
	}
}
