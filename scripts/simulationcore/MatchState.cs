
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
	internal void AddPlayer(PlayerState player)
	{
		_players[player.PlayerId] = player;
	}
}

public sealed class VictoryState
{
	public string WinningPlayerId { get; private set; } = "";
	public string VictoryReason { get; private set; } = "";
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
	protected EntityState(string entityId, string? ownerPlayerId, Vector2 currentPos)
	{
		EntityId = entityId;
		OwnerPlayerId = ownerPlayerId;
		CurrentPosition = currentPos;
		Abilities = new HashSet<Ability>();
	}
	public string EntityId { get; private set; } = "";
	public string? OwnerPlayerId { get; private set; }
	public Vector2 CurrentPosition { get; protected set; }
	public int Health { get; private set; }
	public int MaxHealth { get; private set; }
	public bool GettingRepaired = false;
	public HashSet<Ability> Abilities { get; set; }
}

public class UnitState : EntityState
{
	public UnitState(string entityId, string ownerPlayerId, Vector2 currentPos, float movementSpeed, UnitType unitType = UnitType.BASIC_INFANTRY)
		: base(entityId, ownerPlayerId, currentPos)
	{
		Type = unitType;
		MovementSpeed = movementSpeed;
		Abilities = AbilityCatalog.ForUnit(unitType);
	}
	public UnitType Type { get; private set; }
	public Vector2 TargetPosition { get; private set; }
	public bool HasMoveOrder { get; private set; }
	public int AttackDamage { get; private set; }
	public float AttackRange { get; private set; }
	public float MovementSpeed { get; private set; }
	public int ProductionTime { get; private set; }
	
	internal void SetMoveOrder(Vector2 targetPosition)
	{
		TargetPosition = targetPosition;
		HasMoveOrder = true;
	}
	internal void AdvanceMovement(float deltaSeconds)
	{
		if(!HasMoveOrder)
			return;

		Vector2 direction = TargetPosition - CurrentPosition;
		float distance = direction.Length();

		if (distance <= 2.0f)
		{
			TargetPosition = Vector2.Zero;
			HasMoveOrder = false;
			return;
		}

		CurrentPosition += direction.Normalized() * MovementSpeed * deltaSeconds;
	}
}

public enum BuildingType
{
	BARRACKS,
}

public enum UnitType
{
	BASIC_INFANTRY,
	CONSTRUCTION_UNIT,
	OVERLORD,
}

public sealed class BuildingState : EntityState
{
	public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType buildingType, string constructionUnitId)
		: base(entityId, ownerPlayerId, currentPos)
	{
		Type = buildingType;
		RallyPoint = currentPos + BuildingCatalog.GetFoodprintSize(buildingType);
		Abilities = AbilityCatalog.ForBuilding(buildingType);
		ConstructionUnitId = constructionUnitId;
	}
	public int BuildProgression { get; private set; } = 0;
	internal void AdvanceConstruction(int amount)
	{
		BuildProgression = Math.Min(100, BuildProgression + amount);
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
			//TODO: Spawn Unit
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
	public Vector2 RallyPoint {get; private set; }
	public string ConstructionUnitId { get; private set; }
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
}

public sealed class OutpostState : EntityState
{
	public OutpostState(string entityId, string ownerPlayerId, Vector2 currentPos)
		: base(entityId, ownerPlayerId, currentPos)
	{
	}
	public string OutpostSpecialization { get; private set; } = "";
}

public sealed class ResourceFieldState : EntityState
{
	public ResourceFieldState(string entityId, string ownerPlayerId, Vector2 currentPos)
		: base(entityId, ownerPlayerId, currentPos)
	{
	}
}
