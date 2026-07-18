using System;
using Godot;
public static class BuildingCatalog
{
	/*
	Central place for building footprint sizes.
	Client previews and placement-adjacent logic use the same values.
	*/
	public static Vector2 GetFoodprintSize(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(120, 80),
			BuildingType.RESOURCE_SPAWNER => new Vector2(80, 80),
			BuildingType.RESOURCE_GATHERER => new Vector2(130, 90),
			BuildingType.COMMAND_CENTER => new Vector2(180, 180),
			BuildingType.POWER_PLANT => new Vector2(100, 60),
			BuildingType.WAR_FACTORY => new Vector2(140, 90),
			_ => new Vector2(0, 0)
		};
	}

	/*
	Max health is defined per final building type.
	Construction sites use their own temporary health until completed.
	*/
	public static int GetMaxHealth(BuildingType type)
	{
		return type switch
		{
			BuildingType.CONSTRUCTION_SITE => 200,
			BuildingType.BARRACKS => 500,
			BuildingType.RESOURCE_SPAWNER => 1,
			BuildingType.RESOURCE_GATHERER => 800,
			BuildingType.COMMAND_CENTER => 2000,
			BuildingType.POWER_PLANT => 400,
			BuildingType.WAR_FACTORY => 650,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	/*
	All current buildings use structure armor.
	Keep this method so later building types can diverge in one place.
	*/
	public static ArmorClass GetArmorClass(BuildingType type)
	{
		return ArmorClass.STRUCTURE;
	}

	/*
	Power production provided by each completed building.
	*/
	public static int GetPowerProduction(BuildingType type)
	{
		return type switch
		{
			BuildingType.POWER_PLANT => 10,
			_ => 0
		};
	}

	/*
	Power consumed by each completed building.
	*/
	public static int GetPowerConsumption(BuildingType type)
	{
		return type switch
		{
			BuildingType.WAR_FACTORY => 2,
			_ => 0
		};
	}

	/*
	Construction sites reserve the footprint of the building they will become.
	Completed buildings use their own type directly.
	*/
	public static BuildingType GetFootprintType(BuildingState building)
	{
		return building.Type == BuildingType.CONSTRUCTION_SITE
			? building.pendingBuilding
			: building.Type;
	}

	/*
	Returns true when two centered building footprints overlap.
	Touching edges are allowed so buildings can be placed directly beside each other.
	*/
	public static bool FootprintsOverlap(
		BuildingType firstType,
		Vector2 firstPosition,
		BuildingType secondType,
		Vector2 secondPosition)
	{
		var combinedHalfSize = (GetFoodprintSize(firstType) + GetFoodprintSize(secondType)) / 2f;
		var centerDistance = (firstPosition - secondPosition).Abs();

		return centerDistance.X < combinedHalfSize.X
			&& centerDistance.Y < combinedHalfSize.Y;
	}
}
