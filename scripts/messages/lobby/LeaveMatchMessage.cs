using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class LeaveMatchMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.LEAVE_MATCH;
    public override GDictionary to_payload() => D();
    public override void from_payload(GDictionary payload)
    {
    }
}
