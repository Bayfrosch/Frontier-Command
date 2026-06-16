using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class PauseMatchMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.PAUSE_MATCH;
    public override GDictionary to_payload() => D();
    public override void from_payload(GDictionary payload)
    {
    }
}
