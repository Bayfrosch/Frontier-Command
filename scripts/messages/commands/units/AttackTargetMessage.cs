using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class AttackTargetMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.ATTACK_TARGET;
    public string player_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] unit_ids = System.Array.Empty<string>();
    public string target_id = "";
    public AttackTargetMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, string p_target_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        target_id = p_target_id;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("unit_ids", unit_ids), ("target_id", target_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        unit_ids = PSA(payload, "unit_ids");
        target_id = S(payload, "target_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(unit_ids)) Add(errors, "unit_ids must contain at least one unit.");
        if (Empty(target_id)) Add(errors, "target_id is required.");
        return errors.ToArray();
    }
}
