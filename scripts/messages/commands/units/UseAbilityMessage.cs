using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class UseAbilityMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.USE_ABILITY;
    public string command_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] caster_entity_ids = System.Array.Empty<string>();
    public string ability_id = "";
    public string target_entity_id = "";
    public Variant target_position = default;
    public UseAbilityMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_caster_entity_ids = default, string p_ability_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE, string p_target_entity_id = "", Variant p_target_position = default)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        caster_entity_ids = p_caster_entity_ids ?? System.Array.Empty<string>();
        ability_id = p_ability_id;
        queue_mode = p_queue_mode;
        target_entity_id = p_target_entity_id;
        target_position = p_target_position;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("caster_entity_ids", caster_entity_ids), ("ability_id", ability_id));
        if (!Empty(target_entity_id)) payload["target_entity_id"] = target_entity_id;
        if (!VariantIsNil(target_position)) payload["target_position"] = target_position;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        caster_entity_ids = PSA(payload, "caster_entity_ids");
        ability_id = S(payload, "ability_id");
        target_entity_id = S(payload, "target_entity_id");
        target_position = Var(payload, "target_position");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(caster_entity_ids)) Add(errors, "caster_entity_ids must contain at least one entity.");
        if (Empty(ability_id)) Add(errors, "ability_id is required.");
        if (!Empty(target_entity_id) && !VariantIsNil(target_position)) Add(errors, "only one ability target may be supplied.");
        if (!VariantIsNil(target_position) && !VariantIsVector2(target_position)) Add(errors, "target_position must be a Vector2.");
        return errors.ToArray();
    }
}
