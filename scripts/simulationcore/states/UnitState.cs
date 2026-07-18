using System.Collections.Generic;
using System.Linq;
using Godot;
public class UnitState : EntityState
{
	/*
	Stores movement, combat, construction, and ability state for units.
	*/
	public UnitState(string entityId, string ownerPlayerId, Vector2 currentPos, float movementSpeed, UnitType unitType = UnitType.BASIC_INFANTRY)
		: base(entityId, ownerPlayerId, currentPos, UnitCatalog.GetMaxHealth(unitType), UnitCatalog.GetArmorClass(unitType))
	{
		Type = unitType;
		MovementSpeed = movementSpeed;
		AttackDamage = UnitCatalog.GetAttackDamage(unitType);
		WeaponClass = UnitCatalog.GetWeaponClass(unitType);
		AttackRange = UnitCatalog.GetAttackRange(unitType);
		AttackWindupTime = UnitCatalog.GetAttackWindupTime(unitType);
		AttackCooldownTime = UnitCatalog.GetAttackCooldownTime(unitType);
		Abilities = AbilityCatalog.ForUnit(unitType);
	}
	public UnitType Type { get; private set; }
	public Vector2 TargetPosition { get; private set; }
	public Vector2 CurrentMoveTarget { get; private set; }
	public bool HasMoveOrder { get; private set; }
	public string AttackTargetId { get; private set; } = "";
	public bool HasAttackOrder { get; private set; }
	public Vector2 AttackFormationOffset { get; private set; }
	public string ConstructionTargetId { get; private set; } = "";
	public bool HasConstructionOrder { get; private set; }
	public int AttackDamage { get; private set; }
	public WeaponClass WeaponClass { get; private set; }
	public float AttackRange { get; private set; }
	public float AttackWindupTime { get; private set; }
	public float AttackWindupProgress { get; private set; }
	public float AttackCooldownTime { get; private set; }
	public float AttackCooldownRemaining { get; private set; }
	public bool IsAttackCoolingDown => AttackCooldownRemaining > 0f;
	public float MovementSpeed { get; private set; }
	public int ProductionTime { get; private set; }

	/*
	Sets a movement target and clears incompatible orders unless told to preserve them.
	*/
	internal virtual void SetMoveOrder(Vector2 targetPosition, bool preserveAttackOrder = false, bool preserveConstructionOrder = false)
	{
		TargetPosition = targetPosition;
		CurrentMoveTarget = targetPosition;
		_moveWaypoints.Clear();
		HasMoveOrder = true;
		if (!preserveAttackOrder)
			ClearAttackOrder();
		if (!preserveConstructionOrder)
			ClearConstructionOrder();
	}
	private readonly Queue<Vector2> _moveWaypoints = new();
	/*
	Replaces the immediate path while preserving the final requested target position.
	*/
	internal void SetMovePath(IEnumerable<Vector2> waypoints)
	{
		_moveWaypoints.Clear();

		foreach (var waypoint in waypoints.Where(waypoint => waypoint.DistanceSquaredTo(CurrentPosition) > 1f))
			_moveWaypoints.Enqueue(waypoint);

		CurrentMoveTarget = _moveWaypoints.Count > 0
			? _moveWaypoints.Dequeue()
			: TargetPosition;
	}
	/*
	Stops current movement.
	*/
	internal void ClearMoveOrder()
	{
		TargetPosition = Vector2.Zero;
		CurrentMoveTarget = Vector2.Zero;
		_moveWaypoints.Clear();
		HasMoveOrder = false;
	}
	/*
	Targets an enemy entity and stores formation offset for group attacks.
	*/
	internal void SetAttackOrder(string targetEntityId, Vector2 formationOffset)
	{
		AttackTargetId = targetEntityId;
		AttackFormationOffset = formationOffset;
		HasAttackOrder = true;
		ResetAttackWindup();
	}
	/*
	Clears attack targeting and resets the windup.
	*/
	internal void ClearAttackOrder()
	{
		AttackTargetId = "";
		AttackFormationOffset = Vector2.Zero;
		HasAttackOrder = false;
		ResetAttackWindup();
	}
	/*
	Assigns this unit to work on a construction site.
	*/
	internal void SetConstructionOrder(string targetEntityId)
	{
		ConstructionTargetId = targetEntityId;
		HasConstructionOrder = true;
		ClearAttackOrder();
	}
	/*
	Clears any construction assignment.
	*/
	internal void ClearConstructionOrder()
	{
		ConstructionTargetId = "";
		HasConstructionOrder = false;
	}
	/*
	Returns attack windup to the start.
	*/
	internal void ResetAttackWindup()
	{
		AttackWindupProgress = 0f;
	}
	/*
	Advances attack windup and returns true once the attack should fire.
	*/
	internal bool AdvanceAttackWindup(float deltaSeconds)
	{
		AttackWindupProgress += deltaSeconds;
		return AttackWindupProgress >= AttackWindupTime;
	}
	/*
	Starts the post-attack cooldown.
	*/
	internal void StartAttackCooldown()
	{
		AttackCooldownRemaining = AttackCooldownTime;
	}
	/*
	Reduces attack cooldown over time.
	*/
	internal void AdvanceAttackCooldown(float deltaSeconds)
	{
		AttackCooldownRemaining = AttackCooldownRemaining <= deltaSeconds
			? 0f
			: AttackCooldownRemaining - deltaSeconds;
	}
	/*
	Moves toward the target position and clears the order on arrival.
	*/
	internal void AdvanceMovement(float deltaSeconds)
	{
		if (!HasMoveOrder)
			return;

		var direction = CurrentMoveTarget - CurrentPosition;
		var distance = direction.Length();
		var travelDistance = MovementSpeed * deltaSeconds;

		if (distance <= travelDistance || distance <= 2.0f)
		{
			CurrentPosition = CurrentMoveTarget;
			if (_moveWaypoints.Count > 0)
			{
				CurrentMoveTarget = _moveWaypoints.Dequeue();
				return;
			}

			if (CurrentPosition.DistanceSquaredTo(TargetPosition) <= 4f)
				ClearMoveOrder();
			return;
		}

		CurrentPosition += direction.Normalized() * travelDistance;
	}
}
