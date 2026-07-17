using System;
using Godot;
public sealed class ResourceCollectorState : UnitState
{
	private const int DEFAULT_MAX_CAPACITY = 100;

	/*
	Unit state for harvesters that can carry Virelium between resources and dropoffs.
	*/
	public ResourceCollectorState(string entityId, string ownerPlayerId, Vector2 currentPos, float movementSpeed)
		: base(entityId, ownerPlayerId, currentPos, movementSpeed, UnitType.RESOURCE_COLLECTOR)
	{
	}

	public string ResourceTargetId { get; private set; } = "";
	public string ResourceDropoffBuildingId { get; private set; } = "";
	public bool HasGatherOrder { get; private set; }
	public ResourceCollectorGatherPhase GatherPhase { get; private set; } = ResourceCollectorGatherPhase.Idle;
	public int MaxCapacity { get; private set; } = DEFAULT_MAX_CAPACITY;
	public int Carry { get; private set; }
	public int RemainingCapacity => Math.Max(0, MaxCapacity - Carry);
	public bool HasCargo => Carry > 0;

	/*
	Manual movement cancels gathering so the collector follows the latest player order.
	*/
	internal override void SetMoveOrder(Vector2 targetPosition, bool preserveAttackOrder = false, bool preserveConstructionOrder = false)
	{
		base.SetMoveOrder(targetPosition, preserveAttackOrder, preserveConstructionOrder);
		ClearGatherOrder();
	}

	/*
	Adds material to cargo up to MaxCapacity.
	*/
	internal int Collect(int amount)
	{
		var acceptedAmount = Math.Min(Math.Max(0, amount), RemainingCapacity);
		Carry += acceptedAmount;
		return acceptedAmount;
	}

	/*
	Empties cargo and returns the deposited amount.
	*/
	internal int DepositCargo()
	{
		var depositedAmount = Carry;
		Carry = 0;
		return depositedAmount;
	}

	/*
	Starts a gather loop by moving toward the chosen resource node.
	*/
	internal void SetGatherOrder(string resourceTargetId, Vector2 resourcePosition, string resourceDropoffBuildingId = "")
	{
		ResourceTargetId = resourceTargetId;
		ResourceDropoffBuildingId = resourceDropoffBuildingId;
		HasGatherOrder = true;
		GatherPhase = ResourceCollectorGatherPhase.MovingToResource;
		ClearAttackOrder();
		ClearConstructionOrder();
		base.SetMoveOrder(resourcePosition);
	}

	/*
	Sends the collector back to the resource after depositing cargo.
	*/
	internal void MoveToResource(Vector2 resourcePosition)
	{
		GatherPhase = ResourceCollectorGatherPhase.MovingToResource;
		base.SetMoveOrder(resourcePosition);
	}

	/*
	Sends the collector to the dropoff after taking resources.
	*/
	internal void MoveToDropoff(Vector2 dropoffPosition)
	{
		GatherPhase = ResourceCollectorGatherPhase.ReturningToDropoff;
		base.SetMoveOrder(dropoffPosition);
	}

	/*
	Clears all gather targets and returns to idle phase.
	*/
	internal void ClearGatherOrder()
	{
		ResourceTargetId = "";
		ResourceDropoffBuildingId = "";
		HasGatherOrder = false;
		GatherPhase = ResourceCollectorGatherPhase.Idle;
	}
}
