using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class DisconnectMessage : MessageBase
{
    private static readonly StringName[] VALID_REASONS = {
        "client_quit", "timeout", "kicked", "server_shutdown", "protocol_error"
    };
    public StringName message_type = GameMessages.DISCONNECT;
    public StringName reason = default;
    public DisconnectMessage(StringName p_reason = default)
    {
        reason = p_reason;
    }
    public override GDictionary to_payload() => D(("reason", reason));
    public override void from_payload(GDictionary payload)
    {
        reason = SN(payload, "reason");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (!Contains(VALID_REASONS, reason)) Add(errors, "reason is invalid.");
        return errors.ToArray();
    }
    private static bool Contains(StringName[] values, StringName value)
    {
        foreach (var v in values) if (v == value) return true;
        return false;
    }
}
