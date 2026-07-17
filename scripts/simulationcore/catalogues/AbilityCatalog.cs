using System;
using System.Collections.Generic;
using System.Linq;

public static class AbilityCatalog
{
	/*
	Returns ability ids that each player starts with unlocked.
	Research and progression can add more ids to the player's set.
	*/
	public static HashSet<string> GetDefaultUnlockedAbilityIds()
	{
		return GetAllAbilities()
			.Where(ability => ability.UnlockedFromStart)
			.Select(ability => ability.Id)
			.ToHashSet();
	}

	/*
	Iterates every ability currently defined in the catalog.
	*/
	public static IEnumerable<Ability> GetAllAbilities()
	{
		foreach (var buildingType in Enum.GetValues<BuildingType>())
		{
			foreach (var ability in ForBuilding(buildingType))
				yield return ability;
		}

		foreach (var unitType in Enum.GetValues<UnitType>())
		{
			foreach (var ability in ForUnit(unitType))
				yield return ability;
		}
	}

	/*
	Returns the actions available on a completed building or construction site.
	Cost and RequiresTarget are read by both the simulation and client UI.
	*/
	public static HashSet<Ability> ForBuilding(BuildingType type)
	{
		var abilities = new HashSet<Ability>();
		switch (type)
		{
			case BuildingType.CONSTRUCTION_SITE:
				abilities.Add(new Ability
				{
					Id = "cancel_construction",
					Name = "Abbrechen",
					Cost = 0,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				break;

			case BuildingType.BARRACKS:
				abilities.Add(new Ability
				{
					Id = "sell_building",
					Name = "Verkaufen",
					Cost = 0,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				abilities.Add(new Ability
				{
					Id = "spawn_infantry",
					Name = "Infantry",
					Cost = 120,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				abilities.Add(new Ability
				{
					Id = "spawn_rocket_troops",
					Name = "Rocket Troops",
					Cost = 120,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				break;

			case BuildingType.RESOURCE_GATHERER:
				abilities.Add(new Ability
				{
					Id = "sell_building",
					Name = "Verkaufen",
					Cost = 0,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				abilities.Add(new Ability
				{
					Id = "spawn_resource_collector",
					Name = "Sammler",
					Cost = 150,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				break;
		}
		return abilities;
	}

	/*
	Returns the actions available on units.
	Construction units use target abilities to place new building sites.
	*/
	public static HashSet<Ability> ForUnit(UnitType type)
	{
		var abilities = new HashSet<Ability>();
		switch (type)
		{
			case UnitType.CONSTRUCTION_UNIT:
				abilities.Add(new Ability
				{
					Id = "spawn_barracks",
					Name = "Baracke",
					Cost = 1000,
					RequiresTarget = true,
					UnlockedFromStart = true
				});
				abilities.Add(new Ability
				{
					Id = "spawn_resource_gatherer",
					Name = "Nachschub",
					Cost = 1500,
					RequiresTarget = true,
					UnlockedFromStart = false
				});
				break;

			case UnitType.BASIC_INFANTRY:
				abilities.Add(new Ability
				{
					Id = "capture_building",
					Name = "Einnehmen",
					Cost = 0,
					RequiresTarget = false,
					UnlockedFromStart = true
				});
				break;
		}
		return abilities;
	}
}
