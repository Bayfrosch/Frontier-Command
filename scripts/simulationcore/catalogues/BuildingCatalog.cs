using System;
using System.Linq;
using Godot;
public static class BuildingCatalog
{
	/*
	Footprints match Area2D/CollisionShape2D in the authored building scenes.
	Texture bounds may extend beyond these ground footprints.
	*/
	public static Vector2 GetFoodprintSize(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(239, 131),
			BuildingType.RESOURCE_SPAWNER => new Vector2(80, 80),
			BuildingType.RESOURCE_GATHERER => new Vector2(324, 170),
			BuildingType.COMMAND_CENTER => new Vector2(320, 304),
			BuildingType.POWER_PLANT => new Vector2(183, 86),
			BuildingType.WAR_FACTORY => new Vector2(276, 150),
			BuildingType.RESOURCE => new Vector2(32, 32),
			_ => new Vector2(0, 0)
		};
	}

	public static Vector2 GetFootprintOffset(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(0.5f, 14.5f),
			BuildingType.RESOURCE_GATHERER => new Vector2(0, 11),
			BuildingType.COMMAND_CENTER => new Vector2(70, 62),
			BuildingType.POWER_PLANT => new Vector2(-0.5f, -2),
			BuildingType.WAR_FACTORY => new Vector2(0, 3),
			_ => Vector2.Zero
		};
	}

	public static Rect2 GetFootprintRect(BuildingType type, Vector2 position)
	{
		var size = GetFoodprintSize(type);
		return new Rect2(position + GetFootprintOffset(type) - size / 2f, size);
	}

	public static Rect2 GetFootprintRect(BuildingState building)
	{
		return GetFootprintRect(GetFootprintType(building), building.CurrentPosition);
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
	public static BuildingType GetFootprintType(ICollisionState building)
	{
		return building.Type == BuildingType.CONSTRUCTION_SITE
			? building.pendingBuilding
			: building.Type;
	}

	/*
	Returns true when two building footprints overlap, including their scene offsets.
	Touching edges are allowed so buildings can be placed directly beside each other.
	*/
	public static bool FootprintsOverlap(
		BuildingType firstType,
		Vector2 firstPosition,
		BuildingType secondType,
		Vector2 secondPosition)
	{
		return GetFootprintRect(firstType, firstPosition)
			.Intersects(GetFootprintRect(secondType, secondPosition));
	}

	public static bool IsPlacementValid(IMatchStateView state, BuildingType buildingType, Vector2 position)
	{
		return !state.Players.Values
			.SelectMany(player => player.Entities.Values)
			.OfType<BuildingState>()
			.Any(building => building.Health > 0 && FootprintsOverlap(
				buildingType, position, GetFootprintType(building), building.CurrentPosition))
			&& !state.Resources.Values.Any(resource => !resource.IsDepleted && FootprintsOverlap(
				buildingType, position, GetFootprintType(resource), resource.CurrentPosition));
	}
}
