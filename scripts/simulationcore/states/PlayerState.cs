using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
public sealed class PlayerState
{
	private const int CHEAT_RESOURCE_MINIMUM = 1_000_000;
	/*
	Stores all state that belongs to one player.
	Neutral is also represented as a player for neutral buildings.
	*/
	public PlayerState(string playerId, int startingVirelium = 20000)
	{
		PlayerId = playerId;
		Virelium = startingVirelium;
	}
	public string PlayerId { get; private set; } = "";
	/*
	_entities is the private editable version of the Dictionary
	Entities is the public version which cannot be edited but only read
	*/
	private readonly Dictionary<string, EntityState> _entities = new();
	public IReadOnlyDictionary<string, EntityState> Entities => _entities;
	private readonly Dictionary<string, SquadState> _squads = new();
	public IReadOnlyDictionary<string, SquadState> Squads => _squads;
	/*
	Count of all buildings a player owns
	*/
	private readonly Dictionary<BuildingType, int> _completeBuildingCount = new();
	internal void AddCompletedBuilding(BuildingType type)
	{
		_completeBuildingCount[type] = GetCompleteBuildingCount(type) + 1;
		EnergyProduced += BuildingCatalog.GetPowerProduction(type);
		EnergyConsumed += BuildingCatalog.GetPowerConsumption(type);
		UpdateAbilityUnlocks();
	}
	internal void AddEnergyConsumption(int val)
	{
		EnergyConsumed += val;
	}
	internal void RemoveEnergyConsumption(int val)
	{
		EnergyConsumed -= val;
	}
	internal void RemoveCompletedBuilding(BuildingType type)
	{
		_completeBuildingCount[type] = Math.Max(0, GetCompleteBuildingCount(type) - 1);
		EnergyProduced = Math.Max(0, EnergyProduced - BuildingCatalog.GetPowerProduction(type));
		EnergyConsumed = Math.Max(0, EnergyConsumed - BuildingCatalog.GetPowerConsumption(type));
		UpdateAbilityUnlocks();
	}
	private int GetCompleteBuildingCount(BuildingType type)
	{
		return _completeBuildingCount.TryGetValue(type, out var count) ? count : 0;
	}
	private void UpdateAbilityUnlocks()
	{
		if (GetCompleteBuildingCount(BuildingType.POWER_PLANT) > 0)
		{
			UnlockAbility("spawn_resource_gatherer");
		}
		else
		{
			LockAbility("spawn_resource_gatherer");
		}
	}
	/*
	Adds a unit or building owned by this player.
	*/
	internal void AddEntity(EntityState entity)
	{
		if (entity is UnitState unit)
		{
			ApplyUnitResearch(unit);
			if (unit.SquadId is { } squadId)
			{
				if (!_squads.TryGetValue(squadId, out var squad))
				{
					squad = new SquadState(squadId);
					_squads.Add(squadId, squad);
				}

				squad.AddMember(unit);
			}

		}
		_entities[entity.EntityId] = entity;
	}
	/*
	Removes a unit or building owned by this player.
	*/
	internal bool RemoveEntity(string id)
	{
		if (!_entities.TryGetValue(id, out var entity))
			return false;

		if (entity is UnitState { SquadId: { } squadId } && _squads.TryGetValue(squadId, out var squad))
		{
			squad.RemoveMember(id);

			if (squad.Soldiers.Count == 0)
				_squads.Remove(squadId);
		}

		return _entities.Remove(id);
	}
	/*
	_research is the private editable version of the Dictionary
	Research is the public version which cannot be edited but only read
	*/
	private readonly Dictionary<string, int> _research = new();
	public IReadOnlyDictionary<string, int> Research => _research;
	public int GetResearchLevel(string research) => _research.GetValueOrDefault(research);
	private readonly Dictionary<string, ResearchProjectState> _researchProjects = new();
	public IReadOnlyDictionary<string, ResearchProjectState> ResearchProjects => _researchProjects;
	internal void AddResearchProject(ResearchProjectState project) => _researchProjects.Add(project.ResearchId, project);
	internal bool RemoveResearchProject(string researchId) => _researchProjects.Remove(researchId);
	private readonly HashSet<string> _unlockedAbilities = AbilityCatalog.GetDefaultUnlockedAbilityIds();
	public IReadOnlySet<string> UnlockedAbilities => _unlockedAbilities;
	public int AbilityUnlockRevision { get; private set; }
	/*
	Return the integer value of how many troops spawn on one click based on the base quantity and research
	*/
	internal int GetProductionQuantity(UnitType type)
	{
		int quantity = UnitCatalog.GetBaseQuantity(type);
		if (Research.ContainsKey("infantry_training"))
			quantity++;
		return quantity;
	}
	/*
	Marks research as unlocked for this player.
	*/
	internal void AddResearch(string research, int level = 1)
	{
		_research[research] = Math.Max(GetResearchLevel(research), level);
		if (research == "anti_armor_infantry")
			UnlockAbility("spawn_rocket_troops");
		if (research == "light_tanks")
		{
			UnlockAbility("spawn_war_factory");
			UnlockAbility("spawn_light_tank");
		}
		if (research is "better_weapons" or "support_equipment")
		{
			foreach (var unit in _entities.Values.OfType<UnitState>())
				ApplyUnitResearch(unit);
		}
	}
	private void ApplyUnitResearch(UnitState unit)
	{
		unit.SetAttackDamage(GetAttackDamage(unit.Type));
		if (unit.Type is UnitType.BASIC_INFANTRY or UnitType.RPG_TROOPER)
			unit.ApplyHealthBonus(25 * GetResearchLevel("support_equipment"));
	}
	internal int GetAttackDamage(UnitType type) => UnitCatalog.GetAttackDamage(type)
		+ (type == UnitType.BASIC_INFANTRY && Research.ContainsKey("better_weapons") ? 2 : 0);
	/*
	Marks an ability id as available for this player.
	*/
	internal void UnlockAbility(string abilityId)
	{
		if (!string.IsNullOrEmpty(abilityId) && _unlockedAbilities.Add(abilityId))
			AbilityUnlockRevision++;
	}
	internal void LockAbility(string abilityId)
	{
		if (!string.IsNullOrEmpty(abilityId) && _unlockedAbilities.Remove(abilityId))
			AbilityUnlockRevision++;
	}
	/*
	Returns true when this player can use the ability id.
	*/
	internal bool HasUnlockedAbility(string abilityId)
	{
		return _unlockedAbilities.Contains(abilityId);
	}
	public int Virelium { get; private set; }
	public int EnergyProduced { get; private set; }
	public int EnergyConsumed { get; private set; }
	public bool CheatModeEnabled { get; private set; }
	private bool cheatResourcesGranted;
	internal void SetCheatMode(bool enabled)
	{
		CheatModeEnabled = enabled;
		if (!enabled || cheatResourcesGranted)
			return;

		Virelium = Math.Max(Virelium, CHEAT_RESOURCE_MINIMUM);
		cheatResourcesGranted = true;
	}
	/*
	Adds gathered material to the player's stockpile.
	*/
	internal void AddMaterials(int amount)
	{
		Virelium += Math.Max(0, amount);
	}
	/*
	Spends material without allowing the stockpile to go below zero.
	*/
	internal void RemoveMaterials(int amount)
	{
		Virelium = Math.Max(0, Virelium - amount);
	}
}
