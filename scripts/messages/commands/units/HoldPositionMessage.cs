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
    public string command_id = "";
    public int issued_at_tick = -1;
    public string[] unit_ids = System.Array.Empty<string>();
    public bool enabled = true;
    public HoldPositionMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, bool p_enabled = true)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        enabled = p_enabled;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("unit_ids", unit_ids), ("enabled", enabled));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        unit_ids = PSA(payload, "unit_ids");
        enabled = B(payload, "enabled", true);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(unit_ids)) Add(errors, "unit_ids must contain at least one unit.");
        return errors.ToArray();
    }
}
