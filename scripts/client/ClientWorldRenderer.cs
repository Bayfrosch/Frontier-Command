using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientWorldRenderer : Node
{
	[Export] public PackedScene BuildingScene { get; set; } = null!;

	private LocalSimulationNode simulation = null!;
	private readonly Dictionary<string, TestBuilding> buildingsById = new();

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

		// Build
		foreach (var player in state.Players.Values)
		{
			foreach (var entity in player.Entities.Values)
			{
				if (entity is BuildingState buildingState)
				{
					existingBuildingIds.Add(buildingState.EntityId);
					SyncBuilding(buildingState);
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

	private void SyncBuilding(BuildingState buildingState)
	{
		if (!buildingsById.TryGetValue(buildingState.EntityId, out var building))
		{
			building = BuildingScene.Instantiate<TestBuilding>();
			AddChild(building);
			buildingsById[buildingState.EntityId] = building;
		}

		building.ApplyState(buildingState);
	}
}
