using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class MatchSettingsMessage : MessageBase
{
    public StringName message_type = GameMessages.MATCH_SETTINGS;
    public string match_name = "";
    public int max_players = 2;
    public string map_seed = "seed";
    public string map_template = "standard_1v1";
    public int map_width_tiles = 1;
    public int map_height_tiles = 1;
    public bool allow_pause = true;
    public MatchSettingsMessage(string p_match_name = "", int p_max_players = 2, string p_map_seed = "seed", string p_map_template = "standard_1v1", int p_map_width_tiles = 1, int p_map_height_tiles = 1, bool p_allow_pause = true)
    {
        match_name = p_match_name;
        max_players = p_max_players;
        map_seed = p_map_seed;
        map_template = p_map_template;
        map_width_tiles = p_map_width_tiles;
        map_height_tiles = p_map_height_tiles;
        allow_pause = p_allow_pause;
    }
    public override GDictionary to_payload() => D(("match_name", match_name), ("max_players", max_players), ("map_seed", map_seed), ("map_template", map_template), ("map_width_tiles", map_width_tiles), ("map_height_tiles", map_height_tiles), ("allow_pause", allow_pause));
    public override void from_payload(GDictionary payload)
    {
        match_name = S(payload, "match_name");
        max_players = I(payload, "max_players", 2);
        map_seed = S(payload, "map_seed", "seed");
        map_template = S(payload, "map_template", "standard_1v1");
        map_width_tiles = I(payload, "map_width_tiles", 1);
        map_height_tiles = I(payload, "map_height_tiles", 1);
        allow_pause = B(payload, "allow_pause", true);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(match_name)) Add(errors, "match_name is empty");
        if (max_players <= 0) Add(errors, "max_players must be greater than 0");
        if (Empty(map_seed)) Add(errors, "map_seed is empty");
        if (Empty(map_template)) Add(errors, "map_template is empty");
        if (map_width_tiles <= 0) Add(errors, "map_width_tiles must be greater than 0");
        if (map_height_tiles <= 0) Add(errors, "map_height_tiles must be greater than 0");
        return errors.ToArray();
    }
}
