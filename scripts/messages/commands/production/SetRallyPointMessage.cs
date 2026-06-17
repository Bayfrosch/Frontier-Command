using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class SetRallyPointMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.SET_RALLY_POINT;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string[] producer_entity_ids = System.Array.Empty<string>();
    public Variant target = default;
    public SetRallyPointMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_producer_entity_ids = default, Variant p_target = default)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        producer_entity_ids = p_producer_entity_ids ?? System.Array.Empty<string>();
        target = p_target;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("producer_entity_ids", producer_entity_ids), ("target", target));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        producer_entity_ids = PSA(payload, "producer_entity_ids");
        target = Var(payload, "target");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(producer_entity_ids)) Add(errors, "producer_entity_ids must contain at least one entity.");
        if (VariantIsNil(target)) return errors.ToArray();
        if (!VariantIsDictionary(target))
    {
            Add(errors, "target must be a Dictionary or null.");
            return errors.ToArray();
        }
        var target_dictionary = target.AsGodotDictionary();
        var kind = SN(target_dictionary, "kind");
        if (kind == new StringName("position"))
    {
            if (!target_dictionary.ContainsKey("position") || !VariantIsVector2(target_dictionary["position"])) Add(errors, "target.position must be a Vector2.");
        }
        else if (kind == new StringName("entity"))
    {
            if (Empty(S(target_dictionary, "entity_id"))) Add(errors, "target.entity_id is required.");
        }
        else Add(errors, "target.kind is invalid.");
        return errors.ToArray();
    }
}
