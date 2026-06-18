using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CancelProductionMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.CANCEL_PRODUCTION;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string producer_entity_id = "";
    public string queue_item_id = "";
    public CancelProductionMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_producer_entity_id = "", string p_queue_item_id = "")
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        producer_entity_id = p_producer_entity_id;
        queue_item_id = p_queue_item_id;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("producer_entity_id", producer_entity_id), ("queue_item_id", queue_item_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        producer_entity_id = S(payload, "producer_entity_id");
        queue_item_id = S(payload, "queue_item_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(producer_entity_id)) Add(errors, "producer_entity_id is required.");
        if (Empty(queue_item_id)) Add(errors, "queue_item_id is required.");
        return errors.ToArray();
    }
}
