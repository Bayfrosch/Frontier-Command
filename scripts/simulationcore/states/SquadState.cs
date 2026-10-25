using System.Collections.Generic;
using System.Linq;
using Godot;

public class SquadState
{
    public IReadOnlyDictionary<string, UnitState> Soldiers => _soldiers;
    private readonly Dictionary<string, UnitState> _soldiers = new();
    public int Experience { get; private set; }
    public string SquadId { get; private set; }
    public SquadState(string squadId)
    {
        SquadId = squadId;
    }
    public void AddMember(UnitState unit)
    {
        _soldiers.Add(unit.EntityId, unit);
    }
    public void RemoveMember(string unitId)
    {
        _soldiers.Remove(unitId);
    }
}