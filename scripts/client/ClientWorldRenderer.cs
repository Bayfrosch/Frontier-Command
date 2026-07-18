using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClientWorldRenderer : Node
{
	// Building Scenes
	public PackedScene BasicBarracksScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/Barracks.tscn");
	public PackedScene CommandCenterScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/CommandCenter.tscn");
	public PackedScene ConstructionSiteScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/ConstructionSite.tscn");
	public PackedScene PowerPlantScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/PowerPlant.tscn");
	public PackedScene ResourceSpawnerScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/ResourceSpawner.tscn");
	public PackedScene ResourceGathererScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/Resource_Gatherer.tscn");

	// Resource Scenes
	public PackedScene ResourceSourceScene { get; } = GD.Load<PackedScene>("res://scenes/resources/ResourceSource.tscn");

	// Unit Scenes
	public PackedScene BasicInfantryScene { get; } = GD.Load<PackedScene>("res://scenes/units/basicInfantry.tscn");
	public PackedScene RocketTroopsScene { get; } = GD.Load<PackedScene>("res://scenes/units/rocketTroops.tscn");
	public PackedScene ConstructionUnitScene { get; } = GD.Load<PackedScene>("res://scenes/units/constructionUnit.tscn");
	public PackedScene ResourceCollectorScene { get; } = GD.Load<PackedScene>("res://scenes/units/ResourceCollector.tscn");

	private static readonly Color DefaultUnitColor = new(0.7169325f, 0.3354848f, 0.33498362f, 1f);
	private static readonly Color SelectedUnitColor = new(0.2f, 0.8f, 1f, 1f);
	private LocalSimulationNode simulation = null;
	private GameScene Scene = null;
	private readonly Dictionary<string, ClientBuilding> buildingsById = new();
	private readonly Dictionary<string, ClientUnit> unitsById = new();
	private readonly Dictionary<string, ClientResource> resourcesById = new();

	private PackedScene GetBuildingScene(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => BasicBarracksScene,
			BuildingType.COMMAND_CENTER => CommandCenterScene,
			BuildingType.POWER_PLANT => PowerPlantScene,
			BuildingType.RESOURCE_SPAWNER => ResourceSpawnerScene,
			BuildingType.RESOURCE_GATHERER => ResourceGathererScene,
			BuildingType.CONSTRUCTION_SITE => ConstructionSiteScene,
			_ => throw new Exception("BuildingScene does not exist")
		};
	}

	private PackedScene GetUnitScene(UnitType type)
	{
		return type switch
		{
			UnitType.BASIC_INFANTRY => BasicInfantryScene,
			UnitType.RPG_TROOPER => RocketTroopsScene,
			UnitType.CONSTRUCTION_UNIT => ConstructionUnitScene,
			UnitType.RESOURCE_COLLECTOR => ResourceCollectorScene,
			_ => BasicInfantryScene
		};
	}

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

	private static Color GetDefaultUnitColor(UnitType type)
	{
		return type switch
		{
			UnitType.BASIC_INFANTRY => new Color(0.7169325f, 0.3354848f, 0.33498362f, 1f),
			UnitType.RPG_TROOPER => new Color(0.353f, 0.137f, 0.137f),
			UnitType.CONSTRUCTION_UNIT => new Color(0.2565697f, 0.41133666f, 0.9501857f, 1f),
			UnitType.RESOURCE_COLLECTOR => new Color(0.0f, 0.502f, 0.459f),
			_ => DefaultUnitColor
		};
	}

	private void OnUnitSelection()
	{
		foreach (var unit in unitsById.Values)
		{
			var bodyRender = unit.GetNode<ColorRect>("BodyRender");
			bodyRender.Color = Scene.SelectedEntityIds.Contains(unit.EntityId)
				? SelectedUnitColor
				: GetDefaultUnitColor(unit.Type);
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
		var existingResourceIds = new HashSet<string>();

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
				else if (entity is UnitState unitState)
				{
					existingUnitIds.Add(unitState.EntityId);
					SyncUnit(unitState);
				}
			}
		}

		foreach (var resourceState in state.Resources.Values)
		{
			existingResourceIds.Add(resourceState.EntityId);
			SyncResource(resourceState);
		}

		// Remove Buildings that no longer exist
		foreach (var entityId in buildingsById.Keys.ToArray())
		{
			if (!existingBuildingIds.Contains(entityId))
			{
				buildingsById[entityId].QueueFree();
				buildingsById.Remove(entityId);
			}
		}
		// Remove Units that no longer exist
		foreach (var entityId in unitsById.Keys.ToArray())
		{
			if (!existingUnitIds.Contains(entityId))
			{
				unitsById[entityId].QueueFree();
				unitsById.Remove(entityId);
			}
		}
		// Remove Resources that no longer exist
		foreach (var entityId in resourcesById.Keys.ToArray())
		{
			if (!existingResourceIds.Contains(entityId))
			{
				resourcesById[entityId].QueueFree();
				resourcesById.Remove(entityId);
			}
		}
	}

	private void SyncBuilding(BuildingState buildingState)
	{
		if (buildingsById.TryGetValue(buildingState.EntityId, out var building)
		&& building.Type != buildingState.Type)
		{
			building.QueueFree();
			buildingsById.Remove(buildingState.EntityId);
			building = null;
		}
		if (!buildingsById.TryGetValue(buildingState.EntityId, out var _building))
		{
			var scene = GetBuildingScene(buildingState.Type);
			_building = scene.Instantiate<ClientBuilding>();
			AddChild(_building);
			buildingsById[buildingState.EntityId] = _building;

			_building.ApplySpawnState(buildingState);
			return;
		}
		_building.ApplyState(buildingState);
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

	private void SyncResource(ResourceState resourceState)
	{
		if (!resourcesById.TryGetValue(resourceState.EntityId, out var resource))
		{
			resource = ResourceSourceScene.Instantiate<ClientResource>();
			AddChild(resource);
			resourcesById[resourceState.EntityId] = resource;

			resource.ApplySpawnState(resourceState);
			return;
		}

		resource.ApplyState(resourceState);
	}
}
