using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class ClientHelloMessage : MessageBase
{
    public StringName message_type = GameMessages.CLIENT_HELLO;
    public string client_version = "";
    public string player_name = "";
    public string reconnect_token = "";
    public ClientHelloMessage(string p_client_version = "", string p_player_name = "", string p_reconnect_token = "")
    {
        client_version = p_client_version;
        player_name = p_player_name;
        reconnect_token = p_reconnect_token;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("client_version", client_version), ("player_name", player_name));
        if (!Empty(reconnect_token)) payload["reconnect_token"] = reconnect_token;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        client_version = S(payload, "client_version");
        player_name = S(payload, "player_name");
        reconnect_token = S(payload, "reconnect_token");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_name)) Add(errors, "player_name is required.");
        if (Empty(client_version)) Add(errors, "client_version is required.");
        return errors.ToArray();
    }
}
