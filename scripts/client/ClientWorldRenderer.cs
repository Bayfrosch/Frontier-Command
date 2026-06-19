using Godot;
using System;
using System.Collections.Generic;

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

	private void SyncFromState()
	{
		var state = simulation.GetState();

		foreach (var player in state.Players.Values)
		{
			foreach (var entity in player.Entities.Values)
			{
				if (entity is BuildingState buildingState)
				{
					SyncBuilding(buildingState);
				}
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
