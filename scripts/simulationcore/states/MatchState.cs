
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/*
Top-level match state owned by SimulationContext.
It stores players, neutral resources, projectiles, and global match progress.
*/
public sealed class MatchState : IMatchStateView
{
	public int Tick { get; private set; }
	/*
	Advances the authoritative simulation tick counter.
	*/
	internal void IncrementTick()
	{
		Tick++;
	}
	public VictoryState VictoryState { get; private set; } = new();
	/*
	_players is the private editable version of the Dictionary
	Players is the public version which cannot be edited but only read
	*/
	private readonly Dictionary<string, PlayerState> _players = new();
	public IReadOnlyDictionary<string, PlayerState> Players => _players;
	private readonly Dictionary<string, ResourceState> _resources = new();
	public IReadOnlyDictionary<string, ResourceState> Resources => _resources;
	public readonly Dictionary<string, ProjectileState> Projectiles = new();
	public IReadOnlyDictionary<string, ProjectileState> ProjectilesView => Projectiles;
	/*
	Adds or replaces a player entry by player id.
	*/
	internal void AddPlayer(PlayerState player)
	{
		_players[player.PlayerId] = player;
	}
	/*
	Adds a neutral resource node to the match.
	*/
	internal void AddResource(ResourceState resource)
	{
		_resources[resource.EntityId] = resource;
	}
	/*
	Removes a neutral resource node from the match.
	*/
	internal bool RemoveResource(string id)
	{
		return _resources.Remove(id);
	}
}

public sealed class VictoryState
{
	/*
	Stores the final winner once the match has ended.
	*/
	public string WinningPlayerId { get; private set; } = "";
	public string VictoryReason { get; private set; } = "";
}

/*
Runtime state for one projectile travelling toward an entity.
*/
public sealed class ProjectileState
{
	public string ProjectileId { get; private set; } = "";
	public string SourceEntityId { get; private set; } = "";
	public string SourcePlayerId { get; private set; } = "";
	public string TargetEntityId { get; private set; } = "";
	public string TargetPlayerId { get; private set; } = "";
	public Vector2 CurrentPosition { get; private set; }
	public WeaponDefinition WeaponDefinition { get; private set; }
	/*
	Returns true if the projectile has reached its target and should be removed, false otherwise
	Projectiles live with false and get removed with true.
	*/
	public bool AdvanceMovement(EntityState? TargetEntity, float deltaSeconds)
	{
		if (TargetEntity is null)
			return true;
		
		var toTarget = TargetEntity.CurrentPosition - CurrentPosition;
		var distance = toTarget.Length();
		var travelDistance = WeaponDefinition.ProjectileSpeed * deltaSeconds;

		if (distance <= travelDistance || distance <= 2.0f)
		{
			CurrentPosition = TargetEntity.CurrentPosition;
			return true;
		}
		
		CurrentPosition += toTarget.Normalized() * travelDistance;
		return false;
	}

		
}

public enum WeaponDeliveryType
{
	INSTANT,
	PROJECTILE
}

public enum WeaponClass
{
	SMALL_ARMS,
	CANNON,
	ANTI_ARMOR,
	ANTI_AIR,
	NON_COMBAT,
}

public enum ArmorClass
{
	LIGHT,
	MEDIUM,
	HEAVY,
	STRUCTURE,
	AIR,
}

public sealed class WeaponDefinition
{
	/*
	Static combat data used when a unit or projectile deals damage.
	*/
	public string WeaponId { get; init; }
	public WeaponClass WeaponClass { get; init; }
	public int Damage { get; init; }
	public float Range { get; init; }
	public float Cooldown { get; init; }
	public float WindupTime { get; init; }
	public WeaponDeliveryType DeliveryType { get; init; }
	public float ProjectileSpeed { get; init; }
	public bool Guided { get; init; }
	public float SplashRadius { get; init; }
}

public enum ResourceCollectorGatherPhase
{
	Idle,
	MovingToResource,
	ReturningToDropoff,
}

public enum BuildingType
{
	CONSTRUCTION_SITE,
	BARRACKS,
	RESOURCE_GATHERER,
	RESOURCE_SPAWNER,
	COMMAND_CENTER,
	POWER_PLANT,
	WAR_FACTORY,
}

public enum ResourceType
{
	VIRELIUM,
}

public enum UnitType
{
	// Infantry
	BASIC_INFANTRY,
	RPG_TROOPER,
	// Tanks
	LIGHT_TANK,
	// Non Combat
	CONSTRUCTION_UNIT,
	RESOURCE_COLLECTOR,
}

public sealed class OutpostState : EntityState
{
	/*
	Placeholder state for future outpost specialization logic.
	*/
	public OutpostState(string entityId, string ownerPlayerId, Vector2 currentPos)
		: base(entityId, ownerPlayerId, currentPos, 1, ArmorClass.STRUCTURE)
	{
	}
	public string OutpostSpecialization { get; private set; } = "";
}
