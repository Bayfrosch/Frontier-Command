using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class StartMatchMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.START_MATCH;
    public override GDictionary to_payload() => D();
    public override void from_payload(GDictionary payload)
    {
    }
}
