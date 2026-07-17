using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public sealed class ResourceState : EntityState
{
	/*
	Neutral map resource that collectors can extract from.
	*/
	public ResourceState(
		string entityId,
		ResourceType resourceType,
		string spawnerEntityId,
		Vector2 currentPos,
		int maxAmount
	)
		: base(entityId, null, currentPos, 1, ArmorClass.STRUCTURE)
	{
		ResourceType = resourceType;
		SpawnerEntityId = spawnerEntityId;
		MaxAmount = Math.Max(0, maxAmount);
		CurrentAmount = MaxAmount;
	}
	public ResourceType ResourceType { get; private set; }
	public string SpawnerEntityId { get; private set; }
	public int MaxAmount { get; private set; }
	public int CurrentAmount { get; private set; }
	public bool IsDepleted => CurrentAmount <= 0;

	/*
	Removes up to the requested amount and returns what was actually taken.
	*/
	internal int Extract(int requestedAmount)
	{
		var extractedAmount = Math.Min(Math.Max(0, requestedAmount), CurrentAmount);
		CurrentAmount -= extractedAmount;
		return extractedAmount;
	}
}