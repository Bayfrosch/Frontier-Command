using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class LobbyStateMessage : MessageBase
{
    public StringName message_type = GameMessages.LOBBY_STATE;
    public string lobby_id = "";
    public string host_player_id = "";
    public GDictionary settings = new();
    public GArrayDictionary players = new();
    public LobbyStateMessage(string p_lobby_id = "", string p_host_player_id = "", GDictionary p_settings = null, GArrayDictionary p_players = null)
    {
        lobby_id = p_lobby_id;
        host_player_id = p_host_player_id;
        settings = p_settings ?? new GDictionary();
        players = p_players ?? new GArrayDictionary();
    }
    public override GDictionary to_payload() => D(("lobby_id", lobby_id), ("host_player_id", host_player_id), ("settings", settings), ("players", players));
    public override void from_payload(GDictionary payload)
    {
        lobby_id = S(payload, "lobby_id");
        host_player_id = S(payload, "host_player_id");
        settings = GD(payload, "settings");
        players = GAD(payload, "players");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(lobby_id)) Add(errors, "lobby_id is empty");
        if (Empty(host_player_id)) Add(errors, "host_player_id is empty");
        if (DictStringEmpty(settings, "match_name")) Add(errors, "match_name is missing or empty in settings");
        if (DictIntInvalid(settings, "max_players")) Add(errors, "max_players is missing or invalid in settings");
        if (DictStringEmpty(settings, "map_seed")) Add(errors, "map_seed is missing or empty in settings");
        if (DictStringEmpty(settings, "map_template")) Add(errors, "map_template is missing or empty in settings");
        if (DictIntInvalid(settings, "map_width_tiles")) Add(errors, "map_width_tiles is missing or invalid in settings");
        if (DictIntInvalid(settings, "map_height_tiles")) Add(errors, "map_height_tiles is missing or invalid in settings");
        if (!settings.ContainsKey("allow_pause") || !VariantIsBool(settings["allow_pause"])) Add(errors, "allow_pause is missing or invalid in settings");
        foreach (var player in players)
    {
            if (DictStringEmpty(player, "player_id")) Add(errors, "player_id is missing or empty for a player in players");
            if (DictStringEmpty(player, "display_name")) Add(errors, "display_name is missing or empty for a player in players");
            if (!player.ContainsKey("is_ai") || !VariantIsBool(player["is_ai"])) Add(errors, "is_ai is missing or invalid for a player in players");
            if (!player.ContainsKey("is_ready") || !VariantIsBool(player["is_ready"])) Add(errors, "is_ready is missing or invalid for a player in players");
            if (DictIntNegative(player, "team_id")) Add(errors, "team_id is missing or invalid for a player in players");
            if (DictStringEmpty(player, "faction")) Add(errors, "faction is missing or empty for a player in players");
            if (!player.ContainsKey("color") || !VariantIsColor(player["color"])) Add(errors, "color is missing or invalid for a player in players");
        }
        if (Empty(players)) Add(errors, "players array is empty");
        return errors.ToArray();
    }
}
