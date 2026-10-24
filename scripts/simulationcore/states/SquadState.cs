using System.Collections.Generic;
using Godot;

public class SquadState
{
    public IReadOnlyList<string> SoldierIds { get; }
    public int Experience { get; private set; }
    public string squadId { get; private set; }
}