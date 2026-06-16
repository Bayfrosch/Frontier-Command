using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class PatrolUnitsMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.PATROL_UNITS;
    public string command_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] unit_ids = System.Array.Empty<string>();
    public Vector2 destination = Vector2.Zero;
    public PatrolUnitsMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, Vector2 p_destination = default, int p_queue_mode = (int)GameMessages.QueueMode.REPLACE)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        destination = p_destination;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("unit_ids", unit_ids), ("destination", destination));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        unit_ids = PSA(payload, "unit_ids");
        destination = V2(payload, "destination");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(unit_ids)) Add(errors, "unit_ids must contain at least one unit.");
        return errors.ToArray();
    }
}
