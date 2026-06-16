using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class MessageEnvelope : RefCounted
{
    public const int PROTOCOL_VERSION = 1;
    public int protocol_version = PROTOCOL_VERSION;
    public string message_id = "";
    public StringName message_type = default;
    public int sent_at_unix_ms = -1;
    public GDictionary payload = new();
    public string match_id = "";
    public string sender_player_id = "";
    public string request_id = "";
    public MessageEnvelope(StringName p_message_type = default, GDictionary p_payload = null, string p_message_id = "", int p_sent_at_unix_ms = -1, string p_match_id = "", string p_sender_player_id = "", string p_request_id = "")
    {
        message_type = p_message_type;
        payload = p_payload ?? new GDictionary();
        message_id = p_message_id;
        sent_at_unix_ms = p_sent_at_unix_ms;
        match_id = p_match_id;
        sender_player_id = p_sender_player_id;
        request_id = p_request_id;
    }
    public GDictionary to_payload()
    {
        var envelope = new GDictionary {
            ["protocol_version"] = protocol_version, ["message_id"] = message_id, ["message_type"] = message_type, ["sent_at_unix_ms"] = sent_at_unix_ms, ["payload"] = payload
        };
        if (!string.IsNullOrEmpty(match_id)) envelope["match_id"] = match_id;
        if (!string.IsNullOrEmpty(sender_player_id)) envelope["sender_player_id"] = sender_player_id;
        if (!string.IsNullOrEmpty(request_id)) envelope["request_id"] = request_id;
        return envelope;
    }
    public void from_payload(GDictionary envelope)
    {
        protocol_version = MessageBase.Has(envelope, "protocol_version") ? envelope["protocol_version"].AsInt32():  PROTOCOL_VERSION;
        message_id = MessageBase.Has(envelope, "message_id") ? envelope["message_id"].AsString():  "";
        message_type = MessageBase.Has(envelope, "message_type") ? new StringName(envelope["message_type"].AsString()):  default;
        sent_at_unix_ms = MessageBase.Has(envelope, "sent_at_unix_ms") ? envelope["sent_at_unix_ms"].AsInt32():  -1;
        payload = MessageBase.Has(envelope, "payload") ? envelope["payload"].AsGodotDictionary() : new GDictionary();
        match_id = MessageBase.Has(envelope, "match_id") ? envelope["match_id"].AsString():  "";
        sender_player_id = MessageBase.Has(envelope, "sender_player_id") ? envelope["sender_player_id"].AsString():  "";
        request_id = MessageBase.Has(envelope, "request_id") ? envelope["request_id"].AsString():  "";
    }
    public string[] validate()
    {
        var errors = new List<string>();
        if (protocol_version != PROTOCOL_VERSION) errors.Add("protocol_version is unsupported.");
        if (string.IsNullOrEmpty(message_id)) errors.Add("message_id is required.");
        if (message_type == default || message_type == new StringName("")) errors.Add("message_type is required.");
        if (sent_at_unix_ms < 0) errors.Add("sent_at_unix_ms cannot be negative.");
        return errors.ToArray();
    }
}
