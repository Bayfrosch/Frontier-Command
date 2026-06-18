using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class PingMessage : MessageBase
{
    public StringName message_type = GameMessages.PING;
    public int client_time_unix_ms = -1;
    public PingMessage(int p_client_time_unix_ms = -1)
    {
        client_time_unix_ms = p_client_time_unix_ms;
    }
    public override GDictionary to_payload() => D(("client_time_unix_ms", client_time_unix_ms));
    public override void from_payload(GDictionary payload)
    {
        client_time_unix_ms = I(payload, "client_time_unix_ms", -1);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (client_time_unix_ms < 0) Add(errors, "client_time_unix_ms cannot be negative.");
        return errors.ToArray();
    }
}
