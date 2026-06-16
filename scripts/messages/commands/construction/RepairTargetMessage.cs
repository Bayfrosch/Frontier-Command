using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class RepairTargetMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.REPAIR_TARGET;
    public string command_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] repair_unit_ids = System.Array.Empty<string>();
    public string target_entity_id = "";
    public RepairTargetMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_repair_unit_ids = default, string p_target_entity_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        repair_unit_ids = p_repair_unit_ids ?? System.Array.Empty<string>();
        target_entity_id = p_target_entity_id;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("repair_unit_ids", repair_unit_ids), ("target_entity_id", target_entity_id));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        repair_unit_ids = PSA(payload, "repair_unit_ids");
        target_entity_id = S(payload, "target_entity_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(repair_unit_ids)) Add(errors, "repair_unit_ids must contain at least one unit.");
        if (Empty(target_entity_id)) Add(errors, "target_entity_id is required.");
        return errors.ToArray();
    }
}
