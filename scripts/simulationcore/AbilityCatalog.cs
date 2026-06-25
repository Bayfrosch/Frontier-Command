using System;
using System.Collections.Generic;
using Godot;
public static class AbilityCatalog
{
	public static HashSet<Ability> ForBuilding(BuildingType type)
	{
		var abilities = new HashSet<Ability>();
		switch (type)
		{
			case BuildingType.BARRACKS:
				abilities.Add(new Ability
				{
					Id = "spawn_infantry",
					Name = "Infantry",
					unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_rocket_troops",
					Name = "Rocket Troops",
					unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_sniper",
					Name = "Sniper",
					unlocked = false,
					Cost = 69
				});
				break;
		}
		return abilities;
	}
}