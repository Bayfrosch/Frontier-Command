using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class CreateMatchMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.CREATE_MATCH;
    public GDictionary settings = new();
    public CreateMatchMessage(GDictionary p_settings = null)
    {
        settings = p_settings ?? new GDictionary();
    }
    public override GDictionary to_payload() => D(("settings", settings));
    public override void from_payload(GDictionary payload)
    {
        settings = GD(payload, "settings");
    }
    public override string[] validate()
    {
        var match_settings = new MatchSettingsMessage();
        match_settings.from_payload(settings);
        return match_settings.validate();
    }
}
