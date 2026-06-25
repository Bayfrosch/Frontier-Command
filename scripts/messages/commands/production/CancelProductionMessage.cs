using Godot;
using System;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CancelProductionMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.CANCEL_PRODUCTION;
    public string PlayerId = "";
    public int IssuedAtTick = -1;
    public string ProducerEntityId = "";
    public UnitType QueueItem;
    public CancelProductionMessage(string p_player_id, int p_issued_at_tick, string p_producer_entity_id, UnitType p_queue_item_id)
    {
        PlayerId = p_player_id;
        IssuedAtTick = p_issued_at_tick;
        ProducerEntityId = p_producer_entity_id;
        QueueItem = p_queue_item_id;
    }
    public override GDictionary to_payload() => D(("player_id", PlayerId), ("issued_at_tick", IssuedAtTick), ("producer_entity_id", ProducerEntityId), ("queue_item", Variant.From(QueueItem)));
    public override void from_payload(GDictionary payload)
    {
        PlayerId = S(payload, "player_id");
        IssuedAtTick = I(payload, "issued_at_tick", -1);
        ProducerEntityId = S(payload, "producer_entity_id");
        Enum.TryParse(S(payload, "queue_item"), ignoreCase: true, out QueueItem);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(PlayerId)) Add(errors, "player_id is required.");
        if (IssuedAtTick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(ProducerEntityId)) Add(errors, "producer_entity_id is required.");
        if (Empty(QueueItem.ToString())) Add(errors, "queue_item_id is required.");
        return errors.ToArray();
    }
}
