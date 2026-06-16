using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class SetPlayerReadyMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.SET_PLAYER_READY;
    public bool is_ready = false;
    public string faction_id = "";
    public string team_id = "";
    public SetPlayerReadyMessage(bool p_is_ready = false, string p_faction_id = "", string p_team_id = "")
    {
        is_ready = p_is_ready;
        faction_id = p_faction_id;
        team_id = p_team_id;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("is_ready", is_ready), ("faction_id", faction_id));
        if (!Empty(team_id)) payload["team_id"] = team_id;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        is_ready = B(payload, "is_ready");
        faction_id = S(payload, "faction_id");
        team_id = S(payload, "team_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(faction_id)) Add(errors, "faction_id is required.");
        return errors.ToArray();
    }
}
