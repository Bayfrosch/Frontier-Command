using System;

public static class ResourceCatalog
{
	/*
	Defines the total amount stored in one spawned resource node.
	*/
	public static int GetMaxAmount(ResourceType type)
	{
		return type switch
		{
			ResourceType.VIRELIUM => 2500,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	/*
	Maps world spawner buildings to the resource type they create.
	*/
	public static ResourceType GetSpawnerResourceType(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => ResourceType.VIRELIUM,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	/*
	Controls how many separate resource nodes a spawner creates.
	*/
	public static int GetSpawnerResourceCount(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => 8,
			_ => 0
		};
	}

	/*
	Controls the radius used to place resource nodes around a spawner.
	*/
	public static float GetSpawnerResourceRadius(BuildingType type)
	{
		return type switch
		{
			BuildingType.RESOURCE_SPAWNER => 110f,
			_ => 0f
		};
	}
}
