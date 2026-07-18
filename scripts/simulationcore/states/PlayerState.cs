using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
public sealed class PlayerState
{
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
		if (GetCompleteBuildingCount(BuildingType.BARRACKS) > 0)
		{
			UnlockAbility("spawn_resource_gatherer");
		} else {
			LockAbility("spawn_resource_gatherer");
		}
	}
	/*
	Adds a unit or building owned by this player.
	*/
	internal void AddEntity(EntityState entity)
	{
		_entities[entity.EntityId] = entity;
	}
	/*
	Removes a unit or building owned by this player.
	*/
	internal bool RemoveEntity(string id)
	{
		return _entities.Remove(id);
	}
	/*
	_research is the private editable version of the Dictionary
	Research is the public version which cannot be edited but only read
	*/
	private readonly Dictionary<string, bool> _research = new();
	public IReadOnlyDictionary<string, bool> Research => _research;
	private readonly HashSet<string> _unlockedAbilities = AbilityCatalog.GetDefaultUnlockedAbilityIds();
	public IReadOnlySet<string> UnlockedAbilities => _unlockedAbilities;
	public int AbilityUnlockRevision { get; private set; }
	/*
	Marks research as unlocked for this player.
	*/
	internal void AddResearch(string research)
	{
		_research[research] = true;
	}
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
