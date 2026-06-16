using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class MatchStartedMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.MATCH_STARTED;
    public string match_id = "";
    public int start_tick = -1;
    public int tick_rate = 0;
    public string map_seed = "";
    public string local_player_id = "";
    public MatchStartedMessage(string p_match_id = "", int p_start_tick = -1, int p_tick_rate = 0, string p_map_seed = "", string p_local_player_id = "")
    {
        match_id = p_match_id;
        start_tick = p_start_tick;
        tick_rate = p_tick_rate;
        map_seed = p_map_seed;
        local_player_id = p_local_player_id;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("match_id", match_id), ("start_tick", start_tick), ("tick_rate", tick_rate), ("map_seed", map_seed));
        if (!Empty(local_player_id)) payload["local_player_id"] = local_player_id;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        match_id = S(payload, "match_id");
        start_tick = I(payload, "start_tick", -1);
        tick_rate = I(payload, "tick_rate");
        map_seed = S(payload, "map_seed");
        local_player_id = S(payload, "local_player_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(match_id)) Add(errors, "match_id is required.");
        if (start_tick < 0) Add(errors, "start_tick cannot be negative.");
        if (tick_rate <= 0) Add(errors, "tick_rate must be positive.");
        if (Empty(map_seed)) Add(errors, "map_seed is required.");
        return errors.ToArray();
    }
}
