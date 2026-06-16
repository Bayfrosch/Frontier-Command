using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class GenericErrorMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    private static readonly StringName[] ERROR_CODES = {
        "unsupported_protocol_version", "authentication_failed", "permission_denied", "match_not_found", "match_full", "invalid_match_settings", "player_not_ready", "request_timeout", "internal_server_error"
    };
    public StringName message_type = GameMessages.GENERIC_ERROR;
    public StringName code = default;
    public bool retryable = false;
    public string details = "";
    public GenericErrorMessage(StringName p_code = default, bool p_retryable = false, string p_details = "")
    {
        code = p_code;
        retryable = p_retryable;
        details = p_details;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("code", code), ("retryable", retryable));
        if (!Empty(details)) payload["details"] = details;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        code = SN(payload, "code");
        retryable = B(payload, "retryable");
        details = S(payload, "details");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (!Contains(ERROR_CODES, code)) Add(errors, "code is invalid.");
        return errors.ToArray();
    }
    private static bool Contains(StringName[] values, StringName value)
    {
        foreach (var v in values) if (v == value) return true;
        return false;
    }
}
