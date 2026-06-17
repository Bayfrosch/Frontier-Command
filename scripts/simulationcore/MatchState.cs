public sealed class MatchState
{
    public int Tick { get; set; }
    public Dictionary<string, PlayerState> Players { get; set; } = new Dictionary<string, PlayerState>();
}

public sealed class VictoryState
{
    public string WinningPlayerId { get; set; } = "";
    public string VictoryReason { get; set; } = "";
}

public sealed class PlayerState
{
    public string PlayerId { get; set; } = "";
    public Dictionary<string, EntityState> Entities { get; set; } = new Dictionary<string, EntityState>();
    public Dictionary<string, bool> Research { get; set; } = new Dictionary<string, bool>();
    public int Materials { get; set; }
    public int EnergyProduced { get; set; }
    public int EnergyConsumed { get; set; }
}

public abstract class EntityState
{
    public string EntityId { get; set; } = "";
    public string? OwnerPlayerId { get; set; }
    public Vector2 Position { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }   
}

public sealed class UnitState : EntityState
{
    public string UnitType { get; set; } = "";
    public int AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float MovementSpeed { get; set; }
}

public sealed class BuildingState : EntityState
{
    public string BuildingType { get; set; } = "";
    public int ProductionQueue { get; set; }
    public int ProductionProgress { get; set; }
}

public sealed class OutpostState : EntityState
{
    public string OutpostSpecialization { get; set; } = "";
}

public sealed class ResourceFieldState : EntityState
{
    
}