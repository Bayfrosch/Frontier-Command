using System;
using System.Collections.Generic;
using Godot;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
public static class AbilityCatalog
{
	public static bool RequiresTarget(string abilityId)
	{
		return abilityId switch
		{
			// Units
			"spawn_infantry" => false,
			
			// Buildings
			"spawn_barracks" => true,
			"cancel_construction" => false,
			"sell_building" => false,
			_ => throw new ArgumentOutOfRangeException(abilityId, "abilityId not defined in Catalog")
		};
	}
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
					Cost = 0
				});
				break;

			case BuildingType.BARRACKS:
				abilities.Add(new Ability
				{
					Id = "sell_building",
					Name = "Verkaufen",
					Unlocked = true,
					Cost = 0
				});
				abilities.Add(new Ability
				{
					Id = "spawn_infantry",
					Name = "Infantry",
					Unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_rocket_troops",
					Name = "Rocket Troops",
					Unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_sniper",
					Name = "Sniper",
					Unlocked = false,
					Cost = 69
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
				});
				break;

			case UnitType.BASIC_INFANTRY:
				abilities.Add(new Ability
				{
					Id = "capture_building",
					Name = "Einnehmen",
					Unlocked = true,
					Cost = 0,
				});
				break;
		}
		return abilities;
	}
}
