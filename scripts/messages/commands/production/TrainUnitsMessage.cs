using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class TrainUnitsMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.TRAIN_UNITS;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string producer_entity_id = "";
    public string unit_definition_id = "";
    public int quantity = 1;
    public TrainUnitsMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_producer_entity_id = "", string p_unit_definition_id = "", int p_quantity = 1)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        producer_entity_id = p_producer_entity_id;
        unit_definition_id = p_unit_definition_id;
        quantity = p_quantity;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("producer_entity_id", producer_entity_id), ("unit_definition_id", unit_definition_id), ("quantity", quantity));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        producer_entity_id = S(payload, "producer_entity_id");
        unit_definition_id = S(payload, "unit_definition_id");
        quantity = I(payload, "quantity", 1);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(producer_entity_id)) Add(errors, "producer_entity_id is required.");
        if (Empty(unit_definition_id)) Add(errors, "unit_definition_id is required.");
        if (quantity <= 0) Add(errors, "quantity must be greater than zero.");
        return errors.ToArray();
    }
}
