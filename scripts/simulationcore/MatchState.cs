using System.Collections.Generic;
using Godot;

class ContextProvider
{
    /*
    The state of the match gets saved in this Singleton to prevent issues
    */
    private MatchState matchState = new MatchState();
    /*
    This funtion is for accessing information from the context provider
    */
    public MatchState get()
    {
        return matchState;
    }

    /*
    This function is for accessing what is stored in context provider
    can be accessed with contextProvider.push(...) to store information
    msg Dictionary should consist of pre Defined messages (in scripts/messages)
     */
    public bool push(MessageBase msg)
    {
        return false;
    }
}

public sealed class MatchState
{
    int Tick;
    VictoryState victoryState;
    Dictionary<string, PlayerState> Players = new Dictionary<string, PlayerState>();
}

public sealed class VictoryState
{
    string WinningPlayerId = "";
    string VictoryReason = "";
}

public sealed class PlayerState
{
    string PlayerId = "";
    Dictionary<string, EntityState> Entities = new Dictionary<string, EntityState>();
    Dictionary<string, bool> Research = new Dictionary<string, bool>();
    int Materials;
    int EnergyProduced;
    int EnergyConsumed;
}

public abstract class EntityState
{
    string EntityId = "";
    string? OwnerPlayerId;
    Vector2 Position;
    int Health;
    int MaxHealth;
}

public sealed class UnitState : EntityState
{
    string UnitType = "";
    int AttackDamage;
    float AttackRange;
    float MovementSpeed;
}

public sealed class BuildingState : EntityState
{
    string BuildingType = "";
    int ProductionQueue;
    int ProductionProgress;
}

public sealed class OutpostState : EntityState
{
    string OutpostSpecialization = "";
}

public sealed class ResourceFieldState : EntityState
{

}
