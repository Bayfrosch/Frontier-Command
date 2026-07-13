using System;

public static class ResourceCatalog
{
	public static int GetMaxAmount(ResourceType type)
	{
		return type switch
		{
			ResourceType.MATERIALS => 2500,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public static ResourceType GetSpawnerResourceType(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => ResourceType.MATERIALS,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public static int GetSpawnerResourceCount(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => 8,
			_ => 0
		};
	}

	public static float GetSpawnerResourceRadius(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => 110f,
			_ => 0f
		};
	}
}
