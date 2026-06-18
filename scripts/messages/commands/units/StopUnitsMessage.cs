using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class StopUnitsMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.STOP_UNITS;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string[] unit_ids = System.Array.Empty<string>();
    public StopUnitsMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("unit_ids", unit_ids));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        unit_ids = PSA(payload, "unit_ids");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(unit_ids)) Add(errors, "unit_ids must contain at least one unit.");
        return errors.ToArray();
    }
}
