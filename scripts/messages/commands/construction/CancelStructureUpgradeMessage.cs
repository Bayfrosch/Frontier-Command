using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CancelStructureUpgradeMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.CANCEL_STRUCTURE_UPGRADE;
    public string command_id = "";
    public int issued_at_tick = -1;
    public string structure_entity_id = "";
    public string upgrade_queue_item_id = "";
    public CancelStructureUpgradeMessage(string p_command_id = "", int p_issued_at_tick = -1, string p_structure_entity_id = "", string p_upgrade_queue_item_id = "")
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        structure_entity_id = p_structure_entity_id;
        upgrade_queue_item_id = p_upgrade_queue_item_id;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("structure_entity_id", structure_entity_id), ("upgrade_queue_item_id", upgrade_queue_item_id));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        structure_entity_id = S(payload, "structure_entity_id");
        upgrade_queue_item_id = S(payload, "upgrade_queue_item_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(structure_entity_id)) Add(errors, "structure_entity_id is required.");
        if (Empty(upgrade_queue_item_id)) Add(errors, "upgrade_queue_item_id is required.");
        return errors.ToArray();
    }
}
