using System.Collections.Generic;

public class ClientSquadState
{
    public string SquadId { get; private set; }
    public IReadOnlyList<string> SoldierIds { get; }
    public ClientSquadState (string id, IReadOnlyList<string> soldierIds)
    {
        SquadId = id;
        SoldierIds = soldierIds;
    }
}