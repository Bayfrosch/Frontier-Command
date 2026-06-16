using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class UpdateMatchSettingsMessage : CreateMatchMessage
{
    public new static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public new StringName message_type = GameMessages.UPDATE_MATCH_SETTINGS;
    public UpdateMatchSettingsMessage(GDictionary p_settings = null):  base(p_settings)
    {
    }
}
