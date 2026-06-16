using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class StateDeltaMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.STATE_DELTA;
    public int from_tick = -1;
    public int to_tick = -1;
    public GArrayDictionary changed_players = new();
    public GArrayDictionary upserted_entities = new();
    public string[] removed_entity_ids = System.Array.Empty<string>();
    public string checksum = "";
    public byte[] explored_map_patch = System.Array.Empty<byte>();
    public StateDeltaMessage(int p_from_tick = -1, int p_to_tick = -1, GArrayDictionary p_changed_players = null, GArrayDictionary p_upserted_entities = null, string[] p_removed_entity_ids = default, string p_checksum = "", byte[] p_explored_map_patch = default)
    {
        from_tick = p_from_tick;
        to_tick = p_to_tick;
        changed_players = p_changed_players ?? new GArrayDictionary();
        upserted_entities = p_upserted_entities ?? new GArrayDictionary();
        removed_entity_ids = p_removed_entity_ids ?? System.Array.Empty<string>();
        checksum = p_checksum;
        explored_map_patch = p_explored_map_patch ?? System.Array.Empty<byte>();
    }
    public override GDictionary to_payload()
    {
        var payload = D(("from_tick", from_tick), ("to_tick", to_tick), ("changed_players", changed_players), ("upserted_entities", upserted_entities), ("removed_entity_ids", removed_entity_ids), ("checksum", checksum));
        if (!Empty(explored_map_patch)) payload["explored_map_patch"] = explored_map_patch;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        from_tick = I(payload, "from_tick", -1);
        to_tick = I(payload, "to_tick", -1);
        changed_players = GAD(payload, "changed_players");
        upserted_entities = GAD(payload, "upserted_entities");
        removed_entity_ids = PSA(payload, "removed_entity_ids");
        checksum = S(payload, "checksum");
        explored_map_patch = PBA(payload, "explored_map_patch");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (from_tick < 0) Add(errors, "from_tick cannot be negative.");
        if (to_tick <from_tick) Add(errors, "to_tick cannot be earlier than from_tick.");
        if (Empty(checksum)) Add(errors, "checksum is required.");
        return errors.ToArray();
    }
}
