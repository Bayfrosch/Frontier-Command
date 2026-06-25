using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class UseAbilityMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public StringName message_type = GameMessages.USE_ABILITY;
    public string player_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] caster_entity_ids = System.Array.Empty<string>();
    public string ability_id = "";
    public Vector2? target_position = default;
    public UseAbilityMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_caster_entity_ids = default, string p_ability_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE, Vector2? p_target_position = null)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        caster_entity_ids = p_caster_entity_ids ?? System.Array.Empty<string>();
        ability_id = p_ability_id;
        queue_mode = p_queue_mode;
        target_position = p_target_position;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("caster_entity_ids", caster_entity_ids), ("ability_id", ability_id));
        if (target_position.HasValue) payload["target_position"] = target_position.Value;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        caster_entity_ids = PSA(payload, "caster_entity_ids");
        ability_id = S(payload, "ability_id");
        target_position = V2(payload, "target_position");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(caster_entity_ids)) Add(errors, "caster_entity_ids must contain at least one entity.");
        if (Empty(ability_id)) Add(errors, "ability_id is required.");
        return errors.ToArray();
    }
}
