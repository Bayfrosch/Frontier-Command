using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
public sealed class BuildingState : EntityState, ICollisionState
{
	public sealed record ProductionBatch(UnitType unitType, int quantity);
	internal sealed record PendingSpawn(UnitType UnitType, string? SquadId);
	private const float SPAWN_INTERVAL_SECONDS = 0.4f;
	private readonly Queue<PendingSpawn> _pendingSpawns = new();
	private int _spawnDelayTicks;
	/*
	New buildings start as construction sites and transform when construction completes.
	*/
	public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType pendingBuildingType, string constructionUnitId, int constructionCost = 0)
		: base(entityId, ownerPlayerId, currentPos, BuildingCatalog.GetMaxHealth(BuildingType.CONSTRUCTION_SITE), BuildingCatalog.GetArmorClass(BuildingType.CONSTRUCTION_SITE))
	{
		Type = BuildingType.CONSTRUCTION_SITE;
		RallyPoint = BuildingCatalog.GetFootprintRect(pendingBuildingType, currentPos).End + new Vector2(20f, 20f);
		Abilities = AbilityCatalog.ForBuilding(BuildingType.CONSTRUCTION_SITE);
		ConstructionUnitId = constructionUnitId;
		ConstructionCost = Math.Max(0, constructionCost);
		pendingBuilding = pendingBuildingType;
	}
	public int BuildProgression { get; private set; } = 0;
	/*
	Advances construction and swaps the site into its final building type at 100%.
	*/
	internal void AdvanceConstruction(int amount)
	{
		BuildProgression = Math.Min(100, BuildProgression + amount);

		if (BuildProgression >= 100)
		{
			Type = pendingBuilding;
			SetMaxHealth(BuildingCatalog.GetMaxHealth(pendingBuilding), true);
			Abilities = AbilityCatalog.ForBuilding(pendingBuilding);
		}
	}
	/*
	Advances the first production queue item and returns the finished batch.
	*/
	internal ProductionBatch? AdvanceProduction(int amount)
	{
		if (ProductionQueue.Count <= 0)
			return null;
		var currentUnit = ProductionQueue[0].unitType;
		var productionTime = UnitCatalog.GetProductionTime(currentUnit);
		ProductionProgress = Math.Min(productionTime, ProductionProgress + amount);

		if (ProductionProgress == productionTime)
		{
			var finishedBatch = ProductionQueue[0];
			_productionQueue.RemoveAt(0);
			ProductionProgress = 0;
			return finishedBatch;
		}
		return null;
	}
	internal ProductionBatch[] CompleteProduction()
	{
		var completedBatches = _productionQueue.ToArray();
		_productionQueue.Clear();
		ProductionProgress = 0;
		return completedBatches;
	}
	internal void EnqueueSpawn(ProductionBatch completedBatch, string? squadId)
	{
		for (var i = 0; i < completedBatch.quantity; i++)
			_pendingSpawns.Enqueue(new PendingSpawn(completedBatch.unitType, squadId));
	}
	internal PendingSpawn? AdvanceSpawn(ProductionBatch? completedBatch, string? squadId)
	{
		if (completedBatch is not null)
			EnqueueSpawn(completedBatch, squadId);

		_spawnDelayTicks = Math.Max(0, _spawnDelayTicks - 1);
		return TakeReadySpawn();
	}
	internal PendingSpawn? TakeReadySpawn()
	{
		if (_spawnDelayTicks > 0 || _pendingSpawns.Count == 0)
			return null;

		_spawnDelayTicks = (int)Math.Ceiling(SPAWN_INTERVAL_SECONDS / TimeTickSystem.TICK_DELTA);
		return _pendingSpawns.Dequeue();
	}
	public BuildingType Type { get; set; }
	private readonly List<ProductionBatch> _productionQueue = new();
	public List<ProductionBatch> ProductionQueue => _productionQueue;
	public int ProductionProgress { get; private set; }
	public Vector2 RallyPoint { get; private set; }
	public string ConstructionUnitId { get; private set; }
	public int ConstructionCost { get; private set; }
	public BuildingType pendingBuilding { get; private set; }
	private readonly List<string> _resourceEntityIds = new();
	public IReadOnlyList<string> ResourceEntityIds => _resourceEntityIds;
	public bool HasSpawnedResources { get; private set; }
	/*
	Stores which construction unit is assigned to this site.
	*/
	internal void AssignConstructionUnit(string constructionUnitId)
	{
		ConstructionUnitId = constructionUnitId;
	}
	/*
	Removes the current construction unit assignment.
	*/
	internal void ClearConstructionUnit()
	{
		ConstructionUnitId = "";
	}
	/*
	Sets where produced units should spawn and move.
	*/
	internal void SetRallyPoint(Vector2 newPos)
	{
		RallyPoint = newPos;
	}
	/*
	Removes matching unit types from the production queue.
	*/
	internal void CancelProduction(UnitType unitType)
	{
		_productionQueue.RemoveAll(batch => batch.unitType == unitType);
	}
	/*
	Adds a unit type to the end of the production queue.
	*/
	internal void QueueProduction(UnitType unitType, int quantity)
	{
		_productionQueue.Add(new ProductionBatch(unitType, quantity));
	}
	/*
	Tracks resources spawned by this building.
	*/
	internal void RegisterSpawnedResource(string resourceEntityId)
	{
		_resourceEntityIds.Add(resourceEntityId);
	}
	/*
	Prevents a resource spawner from creating duplicate resource nodes.
	*/
	internal void MarkResourcesSpawned()
	{
		HasSpawnedResources = true;
	}
}
