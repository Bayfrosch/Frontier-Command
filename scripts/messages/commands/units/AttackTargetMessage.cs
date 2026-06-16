using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class AttackTargetMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.ATTACK_TARGET;
    public string command_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] unit_ids = System.Array.Empty<string>();
    public string target_id = "";
    public AttackTargetMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, string p_target_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        target_id = p_target_id;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("unit_ids", unit_ids), ("target_id", target_id));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        unit_ids = PSA(payload, "unit_ids");
        target_id = S(payload, "target_id");
    }
}
