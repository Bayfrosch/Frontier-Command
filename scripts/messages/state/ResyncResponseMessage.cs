using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class ResyncResponseMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.RESYNC_RESPONSE;
    public GDictionary snapshot = new();
    public ResyncResponseMessage(GDictionary p_snapshot = null)
    {
        snapshot = p_snapshot ?? new GDictionary();
    }
    public override GDictionary to_payload() => D(("snapshot", snapshot));
    public override void from_payload(GDictionary payload)
    {
        snapshot = GD(payload, "snapshot");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(snapshot)) Add(errors, "snapshot is required.");
        return errors.ToArray();
    }
}
