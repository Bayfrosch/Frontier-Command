using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class UpdateMatchSettingsMessage : CreateMatchMessage
{
    public new StringName message_type = GameMessages.UPDATE_MATCH_SETTINGS;
    public UpdateMatchSettingsMessage(string p_player_id = "", GDictionary p_settings = null):  base(p_player_id, p_settings)
    {
    }
}
