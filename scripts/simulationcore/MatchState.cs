using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

public sealed class SimulationContext
{
	const float deltaSeconds = 1.0f / 5.0f;
	private const int CONSTRUCTION_ADVANCE = 5;
	public string MatchId { get; }
	/*
	The saved state of the match for each lobby
	can only be edited from SimulationContext, others recieve a readonly
	*/
	private readonly MatchState _matchState = new MatchState();
	/*
	SimulationContext constructor, requires a Match ID
	connecting all message handlers to their specific message type
	*/
	public SimulationContext(string matchId)
	{
		MatchId = matchId;

		// debugging
		Register<DebugSpawnUnitsMessage>(HandleDebugSpawnUnit);

		// construction
		Register<BuildStructureMessage>(HandleBuildStructure);
		Register<CancelConstructionMessage>(HandleCancelConstruction);
		Register<RepairTargetMessage>(HandleRepairTarget);
		Register<CaptureTargetMessage>(HandleCaptureTarget);
		Register<UpgradeStructureMessage>(HandleUpgradeStructure);
		Register<CancelStructureUpgradeMessage>(HandleCancelStructureUpgrade);

		// production
		Register<TrainUnitsMessage>(HandleTrainUnits);
		Register<CancelProductionMessage>(HandleCancelProduction);
		Register<SetRallyPointMessage>(HandleSetRallyPoint);

		// research
		Register<StartResearchMessage>(HandleStartResearch);
		Register<CancelResearchMessage>(HandleCancelResearch);
		Register<ChooseCapitalUpgradeMessage>(HandleChooseCapitalUpgrade);
		Register<SpecializeOutpostMessage>(HandleSpecializeOutpost);

		// units 
		Register<MoveUnitsMessage>(HandleMoveUnit);
		Register<AttackMoveUnitsMessage>(HandleAttackMoveUnits);
		Register<AttackTargetMessage>(HandleAttackTarget);
		Register<StopUnitsMessage>(HandleStopUnits);
		Register<HoldPositionMessage>(HandleHoldPosition);
		Register<PatrolUnitsMessage>(HandlePatrolUnits);
		Register<SetUnitStanceMessage>(HandleSetUnitStance);
		Register<UseAbilityMessage>(HandleUseAbility);
		Register<GatherResourcesMessage>(HandleGatherResources);
	}
	private void Register<TMessage>(Func<TMessage, bool> handler)
		where TMessage : MessageBase
	{
		_handlers[typeof(TMessage)] = msg => handler((TMessage)msg);
	}
	/*
	Processes a Tick by going through all entities
	Advances BuildingState construction by set constant
	*/
	public void AdvanceTick()
	{
		_matchState.IncrementTick();

		foreach (var player in _matchState.Players.Values)
		{
			foreach (var entity in player.Entities.Values)
			{
				if (entity is BuildingState building)
				{
					building.AdvanceConstruction(CONSTRUCTION_ADVANCE);
				} else if (entity is UnitState unit)
				{
					unit.AdvanceMovement(TimeTickSystem.TICK_DELTA);
				}
			}
		}
	}
	/*
	This funtion is for accessing information from the context provider
	Provides a read only Object to get information
	*/
	public IMatchStateView get()
	{
		return _matchState;
	}
	/*
	This function is for accessing what is stored in context provider
	can be accessed with contextProvider.push(...) to store information
	msg Dictionary should consist of pre Defined messages (in scripts/messages)
	 */
	public bool Push(MessageBase msg)
	{
		if (msg is not CommandInterface)
			return false;

		var errors = msg.validate();
		
		if (errors.Length > 0) {
			foreach (var err in errors)
			{
				Console.WriteLine(err);    
			}
			return false;
		}

		if (!_handlers.TryGetValue(msg.GetType(), out var handler))
			return false;

		return handler(msg);
	}
	/*
	Command Handler for directing each message type to the correct handler fuction
	*/
	private readonly Dictionary<System.Type, Func<MessageBase, bool>> _handlers = new();
	/*
	TryGetPlayer gets a playerId as string and gives a PlayerState if found or null
	Returns true if opperation had success and false otherwise
	*/
	private bool TryGetPlayer(string playerId, out PlayerState? player)
	{
		player = null;

		if (string.IsNullOrEmpty(playerId))
			return false;

		return _matchState.Players.TryGetValue(playerId, out player);
	}
	/*
	TryGetPlayerEntity gets a generic which is an EntityState
	It searches for the specific EntityState of the given playerId
	returns true if opperation had success and false otherwise
	*/
	private bool TryGetPlayerEntity<T>(
		string playerId,
		string entityId,
		out T? entity
	) where T : EntityState
	{
		entity = null;

		if (string.IsNullOrEmpty(playerId))
			return false;

		if (!TryGetPlayer(playerId, out var player))
			return false;

		if (player is null)
			return false;

		if (!player.Entities.TryGetValue(entityId, out var rawEntity))
			return false;

		if (rawEntity is not T typedEntity)
			return false;

		entity = typedEntity;

		if (typedEntity is null)
			return false;

		return true;
	}

	private bool TryGetOwnedUnits(
		string playerId,
		string[] unitIds,
		out List<UnitState>? units
	)
	{
		units = new List<UnitState>();

		foreach (var unitId in unitIds)
		{
			if(!TryGetPlayerEntity<UnitState>(playerId, unitId, out var unit))
			{
				units.Clear();
				return false;
			}
			if (unit is null) 
				return false;
			units.Add(unit);   
		}
		return true;
	}
	/*
	Message handlers for reacting to every pre defined message in Messages.cs
	Message types get linked in the constructor to their coresponding handler
	All handlers return true or false regarding wether the message was succesfully parsed
	*/
	private bool HandleMoveUnit(MoveUnitsMessage msg)
	{
		if (!TryGetOwnedUnits(msg.player_id, msg.unit_ids, out var units) || units is null) {
			GD.Print(units is null);
			return false;
		}

		const float spacing = 75f;
		int columns = (int) Math.Ceiling(Math.Sqrt(units.Count));
		int rows = (int) Math.Ceiling(units.Count / (float)columns);

		for (int i = 0; i < units.Count; i++)
		{
			int column = i % columns;
			int row = i / columns;

			float offsetX = (column - (columns - 1) / 2f) * spacing;
			float offsetY = (row - (rows - 1) / 2f) * spacing;

			Vector2 offsetVector = new Vector2(offsetX, offsetY);

			units[i].SetMoveOrder(msg.destination + offsetVector);
		}

		return true;
	}
	private bool HandleBuildStructure(BuildStructureMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		var id = NewEntityId(msg.building_type.ToString());
		BuildingState building = new BuildingState(id, msg.player_id, msg.position, msg.building_type);

		player.AddEntity(building);
		return true;
	}
	private bool HandleCancelConstruction(CancelConstructionMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!TryGetPlayerEntity<BuildingState>(msg.player_id, msg.construction_site_id, out var building) || building is null)
			return false;
		
		if (building.BuildProgression >= 100)
			return false;

		return player.RemoveEntity(msg.construction_site_id);
	}
	private bool HandleRepairTarget(RepairTargetMessage msg)
	{
		if (!TryGetOwnedUnits(msg.player_id, msg.repair_unit_ids, out _))
			return false;

		if (!TryGetPlayerEntity<EntityState>(msg.player_id, msg.target_entity_id, out var entity) || entity is null)
			return false;

		if (entity.Health == entity.MaxHealth)
			return false;

		return entity.GettingRepaired = true;
	}
	// TODO:
	private bool HandleCaptureTarget(CaptureTargetMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleUpgradeStructure(UpgradeStructureMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleCancelStructureUpgrade(CancelStructureUpgradeMessage msg)
	{
		return false;
	}
	private bool HandleTrainUnits(TrainUnitsMessage msg)
	{
		if (!TryGetPlayerEntity<BuildingState>(msg.player_id, msg.producer_entity_id, out var building) || building is null)
			return false;

		for (var i = 0; i < msg.quantity; i++)
			building.QueueProduction(msg.unit_definition_id);

		return true;
	}
	private bool HandleCancelProduction(CancelProductionMessage msg)
	{
		if (!TryGetPlayerEntity<BuildingState>(msg.player_id, msg.producer_entity_id, out var building))
			return false;

		if (building is null)
			return false;

		if (!building.ProductionQueue.Contains(msg.queue_item_id))
			return false;

		building.CancelProduction(msg.queue_item_id);
		return true;
	}
	
	private bool HandleSetRallyPoint(SetRallyPointMessage msg)
	{
		foreach (var entityId in msg.producer_entity_ids)
		{
			if (!TryGetPlayerEntity<BuildingState>(msg.player_id, entityId, out var producer))
				return false;

			producer!.SetRallyPoint(msg.target_position);
		}
		return true;
	}
	// TODO:
	private bool HandleStartResearch(StartResearchMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleCancelResearch(CancelResearchMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleChooseCapitalUpgrade(ChooseCapitalUpgradeMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleSpecializeOutpost(SpecializeOutpostMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleAttackMoveUnits(AttackMoveUnitsMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleAttackTarget(AttackTargetMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleStopUnits(StopUnitsMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleHoldPosition(HoldPositionMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandlePatrolUnits(PatrolUnitsMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleSetUnitStance(SetUnitStanceMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleUseAbility(UseAbilityMessage msg)
	{
		bool passed = false;
		switch (msg.ability_id)
		{
			case "spawn_infantry": 
				passed = HandleSpawnInfantry(msg);
				break;
		}
		return passed;
	}

	private bool HandleSpawnInfantry(UseAbilityMessage msg)
	{
		foreach(string entityId in msg.caster_entity_ids)
		{
			if (!TryGetPlayerEntity(msg.player_id, entityId, out EntityState entity))
				return false;

			if (entity is not BuildingState building)
				return false;

			HandleDebugSpawnUnit(new DebugSpawnUnitsMessage(
				msg.player_id,
				_matchState.Tick,
				UnitType.BASIC_INFANTRY,
				building.RallyPoint,
				10f
			));
		}
		return true;
	}
	// TODO:
	private bool HandleGatherResources(GatherResourcesMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleDebugSpawnUnit(DebugSpawnUnitsMessage msg)
	{
		if (!TryGetPlayer(msg.PlayerId, out var player))
			return false;

		if (player is null)
			return false;

		var unitId = NewEntityId(msg.UnitType.ToString());
		UnitState newUnit = new UnitState(unitId, msg.PlayerId, msg.Position, msg.MovementSpeed, msg.UnitType);
		player.AddEntity(newUnit);
		return true;
	}

	private static string NewEntityId(string prefix)
	{
		return $"{prefix}-{Guid.NewGuid():N}";
	}

	/*
	TODO: Setup Method which can later be replaced by a message handler
	*/
	internal void AddPlayer(PlayerState player)
	{
		_matchState.AddPlayer(player);
	}
}

public interface IMatchStateView
{
	int Tick { get; }
	IReadOnlyDictionary<string, PlayerState> Players { get; }
}

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
public static class BuildingCatalog
{
	public static Vector2 GetFoodprintSize(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(80, 60),
			_ => new Vector2(0, 0)
		};
	}
}
public static class AbilityCatalog
{
	public static HashSet<Ability> ForBuilding(BuildingType type)
	{
		var abilities = new HashSet<Ability>();
		switch (type)
		{
			case BuildingType.BARRACKS:
				abilities.Add(new Ability
				{
					Id = "spawn_infantry",
					Name = "Infantry",
					unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_rocket_troops",
					Name = "Rocket Troops",
					unlocked = true,
					Cost = 20
				});
				abilities.Add(new Ability
				{
					Id = "spawn_sniper",
					Name = "Sniper",
					unlocked = false,
					Cost = 69
				});
				break;
		}
		return abilities;
	}
}

public sealed class UnitState : EntityState
{
	public UnitState(string entityId, string ownerPlayerId, Vector2 currentPos, float movementSpeed, UnitType unitType = UnitType.BASIC_INFANTRY)
		: base(entityId, ownerPlayerId, currentPos)
	{
		Type = unitType;
		MovementSpeed = movementSpeed;
	}
	public UnitType Type { get; private set; }
	public Vector2 TargetPosition { get; private set; }
	public bool HasMoveOrder { get; private set; }
	public int AttackDamage { get; private set; }
	public float AttackRange { get; private set; }
	public float MovementSpeed { get; private set; }
	
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
}

public sealed class BuildingState : EntityState
{
	public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType buildingType)
		: base(entityId, ownerPlayerId, currentPos)
	{
		Type = buildingType;
		RallyPoint = currentPos + BuildingCatalog.GetFoodprintSize(buildingType);
		Abilities = AbilityCatalog.ForBuilding(buildingType);
	}
	public int BuildProgression { get; private set; } = 0;
	internal void AdvanceConstruction(int amount)
	{
		BuildProgression = Math.Min(100, BuildProgression + amount);
	}
	public BuildingType Type;
	public string[] ProductionQueue { get; private set; } = [];
	public int ProductionProgress { get; private set; }
	public Vector2 RallyPoint {get; private set; }
	internal void SetRallyPoint(Vector2 newPos)
	{
		RallyPoint = newPos;
	}
	internal void CancelProduction(string entityId)
	{
		ProductionQueue = ProductionQueue.Where(x => !x.Equals(entityId)).ToArray();
	}
	internal void QueueProduction(string unitDefinitionId)
	{
		ProductionQueue = ProductionQueue.Append(unitDefinitionId).ToArray();
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
