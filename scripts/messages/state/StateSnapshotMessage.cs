using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class StateSnapshotMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.STATE_SNAPSHOT;
    public string match_id = "";
    public int tick = -1;
    public int phase = (int)GameMessages.MatchPhase.LOBBY;
    public int match_time_ms = -1;
    public GArrayDictionary players = new();
    public GArrayDictionary entities = new();
    public byte[] explored_map_data = System.Array.Empty<byte>();
    public string checksum = "";
    public StateSnapshotMessage(string p_match_id = "", int p_tick = -1, int p_phase = (int)GameMessages.MatchPhase.LOBBY, int p_match_time_ms = -1, GArrayDictionary p_players = null, GArrayDictionary p_entities = null, byte[] p_explored_map_data = default, string p_checksum = "")
    {
        match_id = p_match_id;
        tick = p_tick;
        phase = p_phase;
        match_time_ms = p_match_time_ms;
        players = p_players ?? new GArrayDictionary();
        entities = p_entities ?? new GArrayDictionary();
        explored_map_data = p_explored_map_data ?? System.Array.Empty<byte>();
        checksum = p_checksum;
    }
    public override GDictionary to_payload() => D(("match_id", match_id), ("tick", tick), ("phase", phase), ("match_time_ms", match_time_ms), ("players", players), ("entities", entities), ("explored_map_data", explored_map_data), ("checksum", checksum));
    public override void from_payload(GDictionary payload)
    {
        match_id = S(payload, "match_id");
        tick = I(payload, "tick", -1);
        phase = I(payload, "phase", (int)GameMessages.MatchPhase.LOBBY);
        match_time_ms = I(payload, "match_time_ms", -1);
        players = GAD(payload, "players");
        entities = GAD(payload, "entities");
        explored_map_data = PBA(payload, "explored_map_data");
        checksum = S(payload, "checksum");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(match_id)) Add(errors, "match_id is required.");
        if (tick < 0) Add(errors, "tick cannot be negative.");
        if (phase != (int)GameMessages.MatchPhase.LOBBY && phase != (int)GameMessages.MatchPhase.RUNNING && phase != (int)GameMessages.MatchPhase.PAUSED && phase != (int)GameMessages.MatchPhase.ENDED) Add(errors, "phase is invalid.");
        if (match_time_ms < 0) Add(errors, "match_time_ms cannot be negative.");
        if (Empty(checksum)) Add(errors, "checksum is required.");
        return errors.ToArray();
    }
}
