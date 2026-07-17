using System;
using System.Collections.Generic;
using Godot;
public abstract class EntityState
{
	/*
	Base state shared by units, buildings, outposts, and resource nodes.
	*/
	protected EntityState(string entityId, string? ownerPlayerId, Vector2 currentPos, int maxHealth, ArmorClass armorClass)
	{
		EntityId = entityId;
		OwnerPlayerId = ownerPlayerId;
		CurrentPosition = currentPos;
		ArmorClass = armorClass;
		Abilities = new HashSet<Ability>();
		MaxHealth = Math.Max(1, maxHealth);
		Health = MaxHealth;
	}
	public string EntityId { get; private set; } = "";
	public string? OwnerPlayerId { get; private set; }
	public Vector2 CurrentPosition { get; protected set; }
	public ArmorClass ArmorClass { get; private set; }
	public int Health { get; private set; }
	public int MaxHealth { get; private set; }
	public bool GettingRepaired = false;
	public HashSet<Ability> Abilities { get; set; }
	/*
	Applies damage and clamps health at zero.
	*/
	internal void TakeDamage(int damage)
	{
		Health = Math.Max(0, Health - damage);
	}
	/*
	Changes max health when an entity transforms, optionally healing it.
	*/
	protected void SetMaxHealth(int maxHealth, bool healToFull)
	{
		MaxHealth = Math.Max(1, maxHealth);
		Health = healToFull
			? MaxHealth
			: Math.Min(Health, MaxHealth);
	}
}
