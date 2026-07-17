using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public sealed class PlayerState
{
	/*
	Stores all state that belongs to one player.
	Neutral is also represented as a player for neutral buildings.
	*/
	public PlayerState(string playerId, int startingVirelium = 2000)
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
	/*
	Marks research as unlocked for this player.
	*/
	internal void AddResearch(string research)
	{
		_research[research] = true;
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