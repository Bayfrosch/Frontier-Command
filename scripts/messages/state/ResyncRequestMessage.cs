using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class ResyncRequestMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    private static readonly StringName[] VALID_REASONS = {
        "missing_delta", "checksum_mismatch", "reconnect"
    };
    public StringName message_type = GameMessages.RESYNC_REQUEST;
    public int client_tick = -1;
    public string client_checksum = "";
    public StringName reason = default;
    public ResyncRequestMessage(int p_client_tick = -1, string p_client_checksum = "", StringName p_reason = default)
    {
        client_tick = p_client_tick;
        client_checksum = p_client_checksum;
        reason = p_reason;
    }
    public override GDictionary to_payload() => D(("client_tick", client_tick), ("client_checksum", client_checksum), ("reason", reason));
    public override void from_payload(GDictionary payload)
    {
        client_tick = I(payload, "client_tick", -1);
        client_checksum = S(payload, "client_checksum");
        reason = SN(payload, "reason");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (client_tick < 0) Add(errors, "client_tick cannot be negative.");
        if (Empty(client_checksum)) Add(errors, "client_checksum is required.");
        if (!Contains(VALID_REASONS, reason)) Add(errors, "reason is invalid.");
        return errors.ToArray();
    }
    private static bool Contains(StringName[] values, StringName value)
    {
        foreach (var v in values) if (v == value) return true;
        return false;
    }
}
