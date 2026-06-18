using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CreateMatchMessage : MessageBase
{
    public StringName message_type = GameMessages.CREATE_MATCH;
    public string player_id = "";
    public GDictionary settings = new();
    public CreateMatchMessage(string p_player_id = "", GDictionary p_settings = null)
    {
        player_id = p_player_id;
        settings = p_settings ?? new GDictionary();
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("settings", settings));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        settings = GD(payload, "settings");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        var match_settings = new MatchSettingsMessage();
        match_settings.from_payload(settings);
        errors.AddRange(match_settings.validate());
        return errors.ToArray();
    }
}
