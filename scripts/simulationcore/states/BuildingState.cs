using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public sealed class BuildingState : EntityState
{
	/*
	New buildings start as construction sites and transform when construction completes.
	*/
	public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType pendingBuildingType, string constructionUnitId, int constructionCost = 0)
		: base(entityId, ownerPlayerId, currentPos, BuildingCatalog.GetMaxHealth(BuildingType.CONSTRUCTION_SITE), BuildingCatalog.GetArmorClass(BuildingType.CONSTRUCTION_SITE))
	{
		Type = BuildingType.CONSTRUCTION_SITE;
		RallyPoint = currentPos + BuildingCatalog.GetFoodprintSize(pendingBuildingType) / 2f + new Vector2(20f, 20f);
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
	Advances the first production queue item and returns the finished unit type.
	*/
	internal UnitType? AdvanceProduction(int amount)
	{
		if (ProductionQueue.Length <= 0)
			return null;
		var currentUnit = ProductionQueue[0];
		var productionTime = UnitCatalog.GetProductionTime(currentUnit);
		ProductionProgress = Math.Min(productionTime, ProductionProgress + amount);

		if (ProductionProgress == productionTime)
		{
			var finishedUnit = ProductionQueue[0];
			ProductionQueue = ProductionQueue.Skip(1).ToArray();
			ProductionProgress = 0;
			return finishedUnit;
		}
		return null;
	}
	public BuildingType Type;
	public UnitType[] ProductionQueue { get; private set; } = [];
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
	internal void CancelProduction(UnitType entityId)
	{
		ProductionQueue = ProductionQueue.Where(x => !x.Equals(entityId)).ToArray();
	}
	/*
	Adds a unit type to the end of the production queue.
	*/
	internal void QueueProduction(UnitType unitType)
	{
		ProductionQueue = ProductionQueue.Append(unitType).ToArray();
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
