using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class GatherResourcesMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public StringName message_type = GameMessages.GATHER_RESOURCES;
    public string player_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string[] harvester_entity_ids = System.Array.Empty<string>();
    public string resource_field_entity_id = "";
    public string refinery_entity_id = "";
    public GatherResourcesMessage(string p_player_id = "", int p_issued_at_tick = -1, string[] p_harvester_entity_ids = default, string p_resource_field_entity_id = "", int p_queue_mode = (int)GameMessages.QueueMode.REPLACE, string p_refinery_entity_id = "")
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        harvester_entity_ids = p_harvester_entity_ids ?? System.Array.Empty<string>();
        resource_field_entity_id = p_resource_field_entity_id;
        queue_mode = p_queue_mode;
        refinery_entity_id = p_refinery_entity_id;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("harvester_entity_ids", harvester_entity_ids), ("resource_field_entity_id", resource_field_entity_id));
        if (!Empty(refinery_entity_id)) payload["refinery_entity_id"] = refinery_entity_id;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        harvester_entity_ids = PSA(payload, "harvester_entity_ids");
        resource_field_entity_id = S(payload, "resource_field_entity_id");
        refinery_entity_id = S(payload, "refinery_entity_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(harvester_entity_ids)) Add(errors, "harvester_entity_ids must contain at least one entity.");
        if (Empty(resource_field_entity_id)) Add(errors, "resource_field_entity_id is required.");
        return errors.ToArray();
    }
}
