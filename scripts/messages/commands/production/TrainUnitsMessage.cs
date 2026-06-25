using Godot;
using System;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class TrainUnitsMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.TRAIN_UNITS;
    public string PlayerId = "";
    public int IssuedAtTick = -1;
    public string ProducerEntityId = "";
    public UnitType UnitDefinitionId;
    public int quantity = 1;
    private bool HasUnitDefinitionId = false;
    public TrainUnitsMessage()
    {
    }

    public TrainUnitsMessage(UnitType p_unit_definition_id, string p_player_id = "", int p_issued_at_tick = -1, string p_producer_entity_id = "", int p_quantity = 1)
    {
        PlayerId = p_player_id;
        IssuedAtTick = p_issued_at_tick;
        ProducerEntityId = p_producer_entity_id;
        UnitDefinitionId = p_unit_definition_id;
        HasUnitDefinitionId = true;
        quantity = p_quantity;
    }
    public override GDictionary to_payload() => D(("player_id", PlayerId), ("issued_at_tick", IssuedAtTick), ("producer_entity_id", ProducerEntityId), ("unit_definition_id", Variant.From(UnitDefinitionId)), ("quantity", quantity));
    public override void from_payload(GDictionary payload)
    {
        PlayerId = S(payload, "player_id");
        IssuedAtTick = I(payload, "issued_at_tick", -1);
        ProducerEntityId = S(payload, "producer_entity_id");
        HasUnitDefinitionId = Has(payload, "unit_definition_id") && Enum.TryParse(S(payload, "unit_definition_id"), ignoreCase: true, out UnitDefinitionId);
        quantity = I(payload, "quantity", 1);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(PlayerId)) Add(errors, "player_id is required.");
        if (IssuedAtTick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(ProducerEntityId)) Add(errors, "producer_entity_id is required.");
        if (!HasUnitDefinitionId) Add(errors, "unit_definition_id is required.");
        if (quantity <= 0) Add(errors, "quantity must be greater than zero.");
        return errors.ToArray();
    }
}
