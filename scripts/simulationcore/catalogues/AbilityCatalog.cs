using System.Collections.Generic;

public static class AbilityCatalog
{
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
					Unlocked = true,
					Cost = 0,
					RequiresTarget = false
				});
				break;

			case BuildingType.BARRACKS:
				abilities.Add(new Ability
				{
					Id = "sell_building",
					Name = "Verkaufen",
					Unlocked = true,
					Cost = 0,
					RequiresTarget = false
				});
				abilities.Add(new Ability
				{
					Id = "spawn_infantry",
					Name = "Infantry",
					Unlocked = true,
					Cost = 20,
					RequiresTarget = false
				});
				abilities.Add(new Ability
				{
					Id = "spawn_rocket_troops",
					Name = "Rocket Troops",
					Unlocked = true,
					Cost = 20,
					RequiresTarget = false
				});
				break;

			case BuildingType.RESOURCE_SPAWNER:
				abilities.Add(new Ability
				{
					Id = "sell_building",
					Name = "Verkaufen",
					Unlocked = true,
					Cost = 0,
					RequiresTarget = false
				});
				break;
		}
		return abilities;
	}

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
					Unlocked = true,
					Cost = 1000,
					RequiresTarget = true
				});
				abilities.Add(new Ability
				{
					Id = "spawn_resource_spawner",
					Name = "Resource Spawner",
					Unlocked = true,
					Cost = 1000,
					RequiresTarget = true
				});
				break;

			case UnitType.BASIC_INFANTRY:
				abilities.Add(new Ability
				{
					Id = "capture_building",
					Name = "Einnehmen",
					Unlocked = true,
					Cost = 0,
					RequiresTarget = false
				});
				break;
		}
		return abilities;
	}
}
