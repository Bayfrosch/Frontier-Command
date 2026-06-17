using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

public sealed class SimulationContext
{
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

        // construction
        Register<BuildStructureMessage>(HandleBuildStructure);

        // production

        // research

        // units 
        Register<MoveUnitsMessage>(HandleMoveUnit);
    }
    private void Register<TMessage>(Func<TMessage, bool> handler)
        where TMessage : MessageBase
    {
        _handlers[typeof(TMessage)] = msg => handler((TMessage)msg);
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
    public bool push(MessageBase msg)
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
    Message handlers for reacting to every pre defined message in Messages.cs
    Message types get linked in the constructor to their coresponding handler
    All handlers return true or false regarding wether the message was succesfully parsed
    */
    private bool HandleMoveUnit(MoveUnitsMessage msg)
    {
        if (!_matchState.Players.TryGetValue(msg.player_id, out PlayerState? currentPlayer))
            return false;
        
        if (currentPlayer is null)
            return false;
        
        foreach (string s in msg.unit_ids)
        {
            if (!currentPlayer.Entities.TryGetValue(s, out EntityState? entity))
                return false;
            
            if (entity is null)
                return false;
            
            if (entity is not UnitState unit)
                return false;
            
            unit.SetMoveOrder(msg.destination);
        }

        return true;
    }
    private bool HandleBuildStructure(BuildStructureMessage msg)
    {
        if (!_matchState.Players.TryGetValue(msg.player_id, out PlayerState? player))
            return false;
        
        if (player is null) 
            return false;

        var id = newEntityId(msg.building_type.ToString());
        BuildingState building = new BuildingState(id, msg.player_id, msg.position, msg.building_type);

        player.AddEntity(building);
        return true;
    }

    private static string newEntityId(string prefix)
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
    }
    public string EntityId { get; private set; } = "";
    public string? OwnerPlayerId { get; private set; }
    public Vector2 CurrentPosition { get; protected set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
}

public sealed class UnitState : EntityState
{
    public UnitState(string entityId, string ownerPlayerId, Vector2 currentPos)
        : base(entityId, ownerPlayerId, currentPos)
    {
    }
    public string UnitType { get; private set; } = "";
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
            HasMoveOrder = false;
            return;
        }

        CurrentPosition += direction.Normalized() * MovementSpeed * deltaSeconds;
    }
}

public enum BuildingType
{
    BASIC_GENERATOR,
}

public sealed class BuildingState : EntityState
{
    public BuildingState(string entityId, string ownerPlayerId, Vector2 currentPos, BuildingType buildingType)
        : base(entityId, ownerPlayerId, currentPos)
    {
        Type = buildingType;
    }
    public int BuildProgression { get; private set; } = 0;
    public BuildingType Type;
    public int ProductionQueue { get; private set; }
    public int ProductionProgress { get; private set; }
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
