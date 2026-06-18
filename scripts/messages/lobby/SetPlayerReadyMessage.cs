using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class SetPlayerReadyMessage : MessageBase
{
    public StringName message_type = GameMessages.SET_PLAYER_READY;
    public string player_id = "";
    public bool is_ready = false;
    public string faction_id = "";
    public string team_id = "";
    public SetPlayerReadyMessage(string p_player_id = "", bool p_is_ready = false, string p_faction_id = "", string p_team_id = "")
    {
        player_id = p_player_id;
        is_ready = p_is_ready;
        faction_id = p_faction_id;
        team_id = p_team_id;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("player_id", player_id), ("is_ready", is_ready), ("faction_id", faction_id));
        if (!Empty(team_id)) payload["team_id"] = team_id;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        is_ready = B(payload, "is_ready");
        faction_id = S(payload, "faction_id");
        team_id = S(payload, "team_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (Empty(faction_id)) Add(errors, "faction_id is required.");
        return errors.ToArray();
    }
}
