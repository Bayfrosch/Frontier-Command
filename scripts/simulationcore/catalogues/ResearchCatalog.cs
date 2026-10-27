using System;
using System.Collections.Generic;
using System.Linq;

public enum ResearchChapter { Military, Civilian, Fleet }

public sealed record ResearchDefinition(
	string Id, string Name, ResearchChapter Chapter, string Tree, int Generation,
	int MaterialCost, int Energy, int Seconds, string Description, string[] Prerequisites, int[] UpgradeCosts = null)
{
	public int MaxLevel => 1 + (UpgradeCosts?.Length ?? 0);
	public int GetMaterialCost(int level) => level == 1 ? MaterialCost : UpgradeCosts[level - 2];
}

public static class ResearchCatalog
{
	// shortcut: costs and effects are provisional; generation gates belong to the future outpost upgrade ability.
	public static IReadOnlyList<ResearchDefinition> All { get; } = Array.AsReadOnly(new ResearchDefinition[]
	{
		new("infantry_training", "Better Recruitment", ResearchChapter.Military, "Infantry", 1, 1000, 5, 60,
			"Improve recruitment for infantry leading to +1 spawning infantry unit.", []),
		new("anti_armor_infantry", "Anti-armor squads", ResearchChapter.Military, "Infantry", 1, 1200, 10, 60,
			"Unlock RPG Troopers for production in the Barracks to counter tanks and other armored threats.", ["infantry_training"]),
		new("better_weapons", "Better Weapons", ResearchChapter.Military, "Infantry", 1, 1400, 10, 75,
			"Increase attack damage of all existing and newly trained basic infantry by 2.", ["infantry_training"]),
		new("support_equipment", "Support Equipment", ResearchChapter.Military, "Infantry", 1, 1400, 10, 75,
			"Add 25 current and maximum health to all existing and newly trained basic infantry and RPG Troopers per level. Two levels grant up to +50 health.", ["anti_armor_infantry"], [1800]),
		new("light_tanks", "War Factory", ResearchChapter.Military, "Tanks", 1, 1000, 10, 60,
			"Unlock construction of the War Factory and production of Light Tanks.", []),
		new("better_armor", "Better armor", ResearchChapter.Military, "Tanks", 1, 1200, 10, 60,
			"Reduce damage taken by all existing and newly produced tanks by 5% per level, up to 10% with both levels.", ["light_tanks"], [1600]),
		new("artillery", "Field artillery", ResearchChapter.Military, "Tanks", 1, 1400, 10, 75,
			"Unlock basic siege vehicles. Their range helps break defenses, but they need an escort.", ["better_armor"]),
		new("air_operations", "Air operations", ResearchChapter.Military, "Aircraft", 1, 1500, 15, 90,
			"Unlock an airfield and basic aircraft for reconnaissance and battlefield support.", []),
		new("fighter_aircraft", "Air superiority", ResearchChapter.Military, "Aircraft", 1, 1600, 15, 75,
			"Unlock fighters that protect friendly aircraft and contest enemy airspace.", ["air_operations"]),
		new("strike_aircraft", "Strike aircraft", ResearchChapter.Military, "Aircraft", 1, 1800, 15, 90,
			"Unlock aircraft for precision attacks against exposed units and infrastructure.", ["fighter_aircraft"]),
		new("infantry_doctrine", "Infantry doctrine", ResearchChapter.Military, "Infantry", 2, 2000, 15, 120,
			"Commit to numerous, affordable infantry and specialist squads. Completing this choice excludes the tank doctrine.", []),
		new("assault_infantry", "Assault infantry", ResearchChapter.Military, "Infantry", 2, 1800, 10, 90,
			"Unlock tougher infantry specialists to lead attacks alongside basic soldiers.", ["infantry_doctrine"]),
		new("infantry_weapons", "Infantry weapons", ResearchChapter.Military, "Infantry", 2, 2500, 20, 120,
			"Improve infantry damage through a passive upgrade. Exact values remain open.", ["assault_infantry"]),
		new("tank_doctrine", "Tank doctrine", ResearchChapter.Military, "Tanks", 2, 2200, 20, 120,
			"Commit to costly armor with greater strength per unit. Completing this choice excludes the infantry doctrine.", []),
		new("battle_tanks", "Main battle tanks", ResearchChapter.Military, "Tanks", 2, 2400, 20, 120,
			"Unlock a stronger tank variant for direct engagements and sustained frontline pressure.", ["tank_doctrine"]),
		new("armored_support", "Armored support", ResearchChapter.Military, "Tanks", 2, 2200, 20, 100,
			"Unlock durable repair vehicles to sustain armored formations.", ["battle_tanks"]),
		new("advanced_aircraft", "Advanced aircraft", ResearchChapter.Military, "Aircraft", 2, 2200, 20, 100,
			"Improve aircraft durability and unlock more capable airframes for either doctrine.", ["strike_aircraft"]),
		new("air_transports", "Air transports", ResearchChapter.Military, "Aircraft", 2, 2000, 15, 90,
			"Unlock transports to move infantry and support teams across the battlefield.", ["advanced_aircraft"]),
		new("elite_infantry", "Elite infantry", ResearchChapter.Military, "Infantry", 3, 3200, 25, 150,
			"Unlock elite squads that give an infantry army options against advanced threats.", ["infantry_weapons"]),
		new("infantry_pinnacle", "Strategic command", ResearchChapter.Military, "Infantry", 3, 5000, 35, 180,
			"Placeholder for the infantry doctrine's final technology. Its reward is still undecided.", ["elite_infantry"]),
		new("siege_armor", "Siege armor", ResearchChapter.Military, "Tanks", 3, 3000, 25, 150,
			"Unlock heavy siege armor for assaults against fortified positions.", ["armored_support"]),
		new("tank_pinnacle", "Armored supremacy", ResearchChapter.Military, "Tanks", 3, 5500, 40, 180,
			"Placeholder for the tank doctrine's final technology. Its reward is still undecided.", ["siege_armor"]),
		new("heavy_bombers", "Heavy bombers", ResearchChapter.Military, "Aircraft", 3, 3400, 25, 150,
			"Unlock aircraft that threaten fortified positions while remaining vulnerable to fighters and anti-air weapons.", ["air_transports"]),
		new("refining", "Efficient refining", ResearchChapter.Civilian, "Extraction", 1, 800, 5, 45,
			"Unlock construction of Supply Centers to gather and process Materials.", []),
		new("improved_extractors", "Improved extractors", ResearchChapter.Civilian, "Extraction", 1, 1200, 10, 60,
			"Unlock more efficient extractors and increase the economy's harvesting capability.", ["refining"]),
		new("deep_extraction", "Deep extraction", ResearchChapter.Civilian, "Extraction", 1, 1600, 15, 75,
			"Improve extraction from established resource fields and support sustained production.", ["improved_extractors"]),
		new("generators", "Advanced generators", ResearchChapter.Civilian, "Energy", 1, 1000, 5, 60,
			"Unlock improved power infrastructure for advanced production and research.", []),
		new("power_routing", "Power routing", ResearchChapter.Civilian, "Energy", 1, 1200, 5, 60,
			"Improve power distribution across the base and its forward infrastructure.", ["generators"]),
		new("power_efficiency", "Power efficiency", ResearchChapter.Civilian, "Energy", 1, 1500, 10, 75,
			"Reduce the power burden of infrastructure. Exact savings remain open.", ["power_routing"]),
		new("construction_logistics", "Construction logistics", ResearchChapter.Civilian, "Infrastructure", 1, 1000, 5, 45,
			"Improve construction support and the establishment of forward bases.", []),
		new("mobile_repairs", "Mobile repairs", ResearchChapter.Civilian, "Infrastructure", 1, 1000, 10, 60,
			"Unlock repair support for mechanical units fighting away from the base.", ["construction_logistics"]),
		new("field_workshops", "Field workshops", ResearchChapter.Civilian, "Infrastructure", 1, 1400, 10, 75,
			"Unlock forward workshops to sustain construction and repair operations.", ["mobile_repairs"]),
		new("advanced_extractors", "Advanced extractors", ResearchChapter.Civilian, "Extraction", 2, 2300, 20, 100,
			"Unlock the next extractor design for a stronger Material economy.", ["deep_extraction"]),
		new("high_output_reactors", "High-output reactors", ResearchChapter.Civilian, "Energy", 2, 2000, 10, 90,
			"Unlock reactors with greater power capacity for a growing base.", ["power_efficiency"]),
		new("reinforcement_network", "Rapid reinforcement", ResearchChapter.Civilian, "Infrastructure", 2, 1800, 15, 90,
			"Unlock forward reinforcement infrastructure to sustain frontline armies.", ["field_workshops"]),
		new("material_fabrication", "Material fabrication", ResearchChapter.Civilian, "Extraction", 3, 3500, 40, 120,
			"Unlock infrastructure for renewable Material production after natural fields begin to run out.", ["advanced_extractors"]),
		new("fusion_generators", "Fusion generators", ResearchChapter.Civilian, "Energy", 3, 3200, 15, 120,
			"Unlock powerful generators for advanced infrastructure and large research investments.", ["high_output_reactors"]),
		new("automated_construction", "Automated construction", ResearchChapter.Civilian, "Infrastructure", 3, 2800, 20, 120,
			"Improve construction and repair support across established territory.", ["reinforcement_network"]),
		new("fleet_capacity", "Fleet capacity I", ResearchChapter.Fleet, "Fleet capacity", 1, 800, 5, 45,
			"Increase army fleet capacity. Larger fleets will carry an ongoing economic commitment.", []),
		new("capital_construction", "Capital construction", ResearchChapter.Fleet, "Capital capacity", 1, 1800, 15, 90,
			"Unlock production of basic capital units. Their abilities develop through battle experience.", []),
		new("capital_capacity", "Capital capacity I", ResearchChapter.Fleet, "Capital capacity", 1, 1500, 10, 75,
			"Increase the number of capital units the player can field.", ["capital_construction"]),
		new("fleet_capacity_2", "Fleet capacity II", ResearchChapter.Fleet, "Fleet capacity", 2, 1600, 10, 90,
			"Support a larger conventional army, with an increased economic commitment.", ["fleet_capacity"]),
		new("capital_capacity_2", "Capital capacity II", ResearchChapter.Fleet, "Capital capacity", 2, 2300, 15, 100,
			"Expand the capital unit limit for a growing military force.", ["capital_capacity"]),
		new("fleet_capacity_3", "Fleet capacity III", ResearchChapter.Fleet, "Fleet capacity", 3, 3200, 15, 120,
			"Unlock the largest provisional conventional fleet size.", ["fleet_capacity_2"]),
		new("capital_capacity_3", "Capital capacity III", ResearchChapter.Fleet, "Capital capacity", 3, 4000, 20, 150,
			"Unlock the largest provisional capital unit capacity.", ["capital_capacity_2"])
	});

	private static readonly Dictionary<string, ResearchDefinition> ById = All.ToDictionary(technology => technology.Id);

	public static bool TryGet(string id, out ResearchDefinition definition) => ById.TryGetValue(id, out definition);

	public static int CompletedCoreTrees(PlayerState player, ResearchChapter chapter) => All
		.Where(technology => technology.Chapter == chapter && technology.Generation == 1)
		.GroupBy(technology => technology.Tree)
		.Count(tree => tree.All(technology => player.Research.ContainsKey(technology.Id)));

	public static string LockReason(PlayerState player, ResearchDefinition technology)
	{
		if (technology.Id is "infantry_doctrine" or "tank_doctrine")
		{
			var other = technology.Id == "infantry_doctrine" ? "tank_doctrine" : "infantry_doctrine";
			if (player.Research.ContainsKey(other))
				return "The other doctrine has already been chosen.";
			var military = CompletedCoreTrees(player, ResearchChapter.Military);
			var civilian = CompletedCoreTrees(player, ResearchChapter.Civilian);
			if (military < 2 || civilian < 1)
				return $"Complete 2 Military core trees and 1 Civilian core tree. Current: {military}/2 Military, {civilian}/1 Civilian.";
		}
		var missing = technology.Prerequisites.Where(id => !player.Research.ContainsKey(id)).ToArray();
		return missing.Length == 0 ? "" : "Requires: " + string.Join(", ", missing.Select(id => ById[id].Name));
	}
}
