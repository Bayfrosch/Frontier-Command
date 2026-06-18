using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class JoinMatchMessage : MessageBase
{
    public StringName message_type = GameMessages.JOIN_MATCH;
    public string player_id = "";
    public string match_id = "";
    public bool as_observer = false;
    public JoinMatchMessage(string p_player_id = "", string p_match_id = "", bool p_as_observer = false)
    {
        player_id = p_player_id;
        match_id = p_match_id;
        as_observer = p_as_observer;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("match_id", match_id), ("as_observer", as_observer));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        match_id = S(payload, "match_id");
        as_observer = B(payload, "as_observer");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (Empty(match_id)) Add(errors, "match_id is required.");
        return errors.ToArray();
    }
}
