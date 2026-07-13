using System;
using Godot;
public static class BuildingCatalog
{
	public static Vector2 GetFoodprintSize(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(80, 60),
			_ => new Vector2(0, 0)
		};
	}

	public static int GetMaxHealth(BuildingType type)
	{
		return type switch
		{
			BuildingType.CONSTRUCTION_SITE => 200,
			BuildingType.BARRACKS => 500,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public static ArmorClass GetArmorClass(BuildingType type)
	{
		return ArmorClass.STRUCTURE;
	}

	public static BuildingType GetFootprintType(BuildingState building)
	{
		return building.Type == BuildingType.CONSTRUCTION_SITE
			? building.pendingBuilding
			: building.Type;
	}
}
