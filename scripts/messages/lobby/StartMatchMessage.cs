using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class StartMatchMessage : MessageBase
{
    public StringName message_type = GameMessages.START_MATCH;
    public string player_id = "";
    public StartMatchMessage(string p_player_id = "")
    {
        player_id = p_player_id;
    }
    public override GDictionary to_payload() => D(("player_id", player_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        return errors.ToArray();
    }
}
