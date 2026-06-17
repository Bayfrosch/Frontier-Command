using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class HoldPositionMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.HOLD_POSITION_UNITS;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string[] unit_ids = System.Array.Empty<string>();
    public bool enabled = true;
    public HoldPositionMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, bool p_enabled = true)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        enabled = p_enabled;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("unit_ids", unit_ids), ("enabled", enabled));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        unit_ids = PSA(payload, "unit_ids");
        enabled = B(payload, "enabled", true);
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
