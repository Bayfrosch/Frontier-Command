using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class SetRallyPointMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.SET_RALLY_POINT;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string[] producer_entity_ids = System.Array.Empty<string>();
    public Vector2 target_position = Vector2.Zero;
    public SetRallyPointMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_producer_entity_ids = default, Vector2 p_target_position = default)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        producer_entity_ids = p_producer_entity_ids ?? System.Array.Empty<string>();
        target_position = p_target_position;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("producer_entity_ids", producer_entity_ids), ("target_position", target_position));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        producer_entity_ids = PSA(payload, "producer_entity_ids");
        target_position = V2(payload, "target_position");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(producer_entity_ids)) Add(errors, "producer_entity_ids must contain at least one entity.");
        return errors.ToArray();
    }
}
