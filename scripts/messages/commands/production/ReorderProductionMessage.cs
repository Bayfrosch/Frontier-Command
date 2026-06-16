using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class ReorderProductionMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.REORDER_PRODUCTION;
    public string command_id = "";
    public int issued_at_tick = -1;
    public string producer_entity_id = "";
    public string queue_item_id = "";
    public int new_index = -1;
    public ReorderProductionMessage(string p_command_id = "", int p_issued_at_tick = -1, string p_producer_entity_id = "", string p_queue_item_id = "", int p_new_index = -1)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        producer_entity_id = p_producer_entity_id;
        queue_item_id = p_queue_item_id;
        new_index = p_new_index;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("producer_entity_id", producer_entity_id), ("queue_item_id", queue_item_id), ("new_index", new_index));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        producer_entity_id = S(payload, "producer_entity_id");
        queue_item_id = S(payload, "queue_item_id");
        new_index = I(payload, "new_index", -1);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(producer_entity_id)) Add(errors, "producer_entity_id is required.");
        if (Empty(queue_item_id)) Add(errors, "queue_item_id is required.");
        if (new_index < 0) Add(errors, "new_index cannot be negative.");
        return errors.ToArray();
    }
}
