using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class JoinMatchMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.JOIN_MATCH;
    public string match_id = "";
    public bool as_observer = false;
    public JoinMatchMessage(string p_match_id = "", bool p_as_observer = false)
    {
        match_id = p_match_id;
        as_observer = p_as_observer;
    }
    public override GDictionary to_payload() => D(("match_id", match_id), ("as_observer", as_observer));
    public override void from_payload(GDictionary payload)
    {
        match_id = S(payload, "match_id");
        as_observer = B(payload, "as_observer");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(match_id)) Add(errors, "match_id is required.");
        return errors.ToArray();
    }
}
