
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public sealed class MatchState : IMatchStateView
{
	public int Tick { get; private set; }
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
	internal void AddPlayer(PlayerState player)
	{
		_players[player.PlayerId] = player;
	}
	internal void AddResource(ResourceState resource)
	{
		_resources[resource.EntityId] = resource;
	}
	internal bool RemoveResource(string id)
	{
		return _resources.Remove(id);
	}
}

public sealed class VictoryState
{
	public string WinningPlayerId { get; private set; } = "";
	public string VictoryReason { get; private set; } = "";
}

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

public sealed class PlayerState
{
	public PlayerState(string playerId)
	{
		PlayerId = playerId;
	}
	public string PlayerId { get; private set; } = "";
	/*
	_entities is the private editable version of the Dictionary
	Entities is the public version which cannot be edited but only read
	*/
	private readonly Dictionary<string, EntityState> _entities = new();
	public IReadOnlyDictionary<string, EntityState> Entities => _entities;
	internal void AddEntity(EntityState entity)
	{
		_entities[entity.EntityId] = entity;
	}
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
	internal void AddResearch(string research)
	{
		_research[research] = true;
	}
	public int Materials { get; private set; }
	public int EnergyProduced { get; private set; }
	public int EnergyConsumed { get; private set; }
}

public abstract class EntityState
{
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
	internal void TakeDamage(int damage)
	{
		Health = Math.Max(0, Health - damage);
	}
	protected void SetMaxHealth(int maxHealth, bool healToFull)
	{
		MaxHealth = Math.Max(1, maxHealth);
		Health = healToFull
			? MaxHealth
			: Math.Min(Health, MaxHealth);
	}
}

public class UnitState : EntityState
{
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

	internal void SetMoveOrder(Vector2 targetPosition, bool preserveAttackOrder = false, bool preserveConstructionOrder = false)
	{
		TargetPosition = targetPosition;
		HasMoveOrder = true;
		if (!preserveAttackOrder)
			ClearAttackOrder();
		if (!preserveConstructionOrder)
			ClearConstructionOrder();
	}
	internal void ClearMoveOrder()
	{
		TargetPosition = Vector2.Zero;
		HasMoveOrder = false;
	}
	internal void SetAttackOrder(string targetEntityId, Vector2 formationOffset)
	{
		AttackTargetId = targetEntityId;
		AttackFormationOffset = formationOffset;
		HasAttackOrder = true;
		ResetAttackWindup();
	}
	internal void ClearAttackOrder()
	{
		AttackTargetId = "";
		AttackFormationOffset = Vector2.Zero;
		HasAttackOrder = false;
		ResetAttackWindup();
	}
	internal void SetConstructionOrder(string targetEntityId)
	{
		ConstructionTargetId = targetEntityId;
		HasConstructionOrder = true;
		ClearAttackOrder();
	}
	internal void ClearConstructionOrder()
	{
		ConstructionTargetId = "";
		HasConstructionOrder = false;
	}
	internal void ResetAttackWindup()
	{
		AttackWindupProgress = 0f;
	}
	internal bool AdvanceAttackWindup(float deltaSeconds)
	{
		AttackWindupProgress += deltaSeconds;
		return AttackWindupProgress >= AttackWindupTime;
	}
	internal void StartAttackCooldown()
	{
		AttackCooldownRemaining = AttackCooldownTime;
	}
	internal void AdvanceAttackCooldown(float deltaSeconds)
	{
		AttackCooldownRemaining = AttackCooldownRemaining <= deltaSeconds
			? 0f
			: AttackCooldownRemaining - deltaSeconds;
	}
	internal void AdvanceMovement(float deltaSeconds)
	{
		if (!HasMoveOrder)
			return;

		var direction = TargetPosition - CurrentPosition;
		var distance = direction.Length();
		var travelDistance = MovementSpeed * deltaSeconds;

		if (distance <= travelDistance || distance <= 2.0f)
		{
			CurrentPosition = TargetPosition;
			ClearMoveOrder();
			return;
		}

		CurrentPosition += direction.Normalized() * travelDistance;
	}
}

public enum BuildingType
{
	CONSTRUCTION_SITE,
	BARRACKS,
	RESOURCE_GATHERER,
	RESOURCE_SPAWNER,
}

public enum ResourceType
{
	MATERIALS,
}

public enum UnitType
{
	// Infantry
	BASIC_INFANTRY,
	RPG_TROOPER,
	// Tanks
	// Non Combat
	CONSTRUCTION_UNIT,
}

public sealed class BuildingState : EntityState
{
	public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType pendingBuildingType, string constructionUnitId)
		: base(entityId, ownerPlayerId, currentPos, BuildingCatalog.GetMaxHealth(BuildingType.CONSTRUCTION_SITE), BuildingCatalog.GetArmorClass(BuildingType.CONSTRUCTION_SITE))
	{
		Type = BuildingType.CONSTRUCTION_SITE;
		RallyPoint = currentPos + BuildingCatalog.GetFoodprintSize(pendingBuildingType) / 2f + new Vector2(20f, 20f);
		Abilities = AbilityCatalog.ForBuilding(BuildingType.CONSTRUCTION_SITE);
		ConstructionUnitId = constructionUnitId;
		pendingBuilding = pendingBuildingType;
	}
	public int BuildProgression { get; private set; } = 0;
	internal void AdvanceConstruction(int amount)
	{
		BuildProgression = Math.Min(100, BuildProgression + amount);

		if (BuildProgression >= 100)
		{
			Type = pendingBuilding;
			SetMaxHealth(BuildingCatalog.GetMaxHealth(pendingBuilding), true);
			Abilities = AbilityCatalog.ForBuilding(pendingBuilding);
		}
	}
	internal UnitType? AdvanceProduction(int amount)
	{
		if (ProductionQueue.Length <= 0)
			return null;
		var currentUnit = ProductionQueue[0];
		var productionTime = UnitCatalog.GetProductionTime(currentUnit);
		ProductionProgress = Math.Min(productionTime, ProductionProgress + amount);

		if (ProductionProgress == productionTime)
		{
			var finishedUnit = ProductionQueue[0];
			ProductionQueue = ProductionQueue.Skip(1).ToArray();
			ProductionProgress = 0;
			return finishedUnit;
		}
		return null;
	}
	public BuildingType Type;
	public UnitType[] ProductionQueue { get; private set; } = [];
	public int ProductionProgress { get; private set; }
	public Vector2 RallyPoint { get; private set; }
	public string ConstructionUnitId { get; private set; }
	public BuildingType pendingBuilding { get; private set; }
	private readonly List<string> _resourceEntityIds = new();
	public IReadOnlyList<string> ResourceEntityIds => _resourceEntityIds;
	public bool HasSpawnedResources { get; private set; }
	internal void AssignConstructionUnit(string constructionUnitId)
	{
		ConstructionUnitId = constructionUnitId;
	}
	internal void ClearConstructionUnit()
	{
		ConstructionUnitId = "";
	}
	internal void SetRallyPoint(Vector2 newPos)
	{
		RallyPoint = newPos;
	}
	internal void CancelProduction(UnitType entityId)
	{
		ProductionQueue = ProductionQueue.Where(x => !x.Equals(entityId)).ToArray();
	}
	internal void QueueProduction(UnitType unitType)
	{
		ProductionQueue = ProductionQueue.Append(unitType).ToArray();
	}
	internal void RegisterSpawnedResource(string resourceEntityId)
	{
		_resourceEntityIds.Add(resourceEntityId);
	}
	internal void MarkResourcesSpawned()
	{
		HasSpawnedResources = true;
	}
}

public sealed class OutpostState : EntityState
{
	public OutpostState(string entityId, string ownerPlayerId, Vector2 currentPos)
		: base(entityId, ownerPlayerId, currentPos, 1, ArmorClass.STRUCTURE)
	{
	}
	public string OutpostSpecialization { get; private set; } = "";
}

public sealed class ResourceState : EntityState
{
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

	internal int Extract(int requestedAmount)
	{
		var extractedAmount = Math.Min(Math.Max(0, requestedAmount), CurrentAmount);
		CurrentAmount -= extractedAmount;
		return extractedAmount;
	}
}
