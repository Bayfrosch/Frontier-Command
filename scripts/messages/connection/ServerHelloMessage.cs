using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class ServerHelloMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    public StringName message_type = GameMessages.SERVER_HELLO;
    public string server_version = "";
    public int protocol_version = -1;
    public string player_id = "";
    public string reconnect_token = "";
    public int server_time_unix_ms = -1;
    public ServerHelloMessage(string p_server_version = "", int p_protocol_version = -1, string p_player_id = "", string p_reconnect_token = "", int p_server_time_unix_ms = -1)
    {
        server_version = p_server_version;
        protocol_version = p_protocol_version;
        player_id = p_player_id;
        reconnect_token = p_reconnect_token;
        server_time_unix_ms = p_server_time_unix_ms;
    }
    public override GDictionary to_payload() => D(("server_version", server_version), ("protocol_version", protocol_version), ("player_id", player_id), ("reconnect_token", reconnect_token), ("server_time_unix_ms", server_time_unix_ms));
    public override void from_payload(GDictionary payload)
    {
        server_version = S(payload, "server_version");
        protocol_version = I(payload, "protocol_version", -1);
        player_id = S(payload, "player_id");
        reconnect_token = S(payload, "reconnect_token");
        server_time_unix_ms = I(payload, "server_time_unix_ms", -1);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(server_version)) Add(errors, "server_version is required.");
        if (protocol_version < 1) Add(errors, "protocol_version must be positive.");
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (Empty(reconnect_token)) Add(errors, "reconnect_token is required.");
        if (server_time_unix_ms < 0) Add(errors, "server_time_unix_ms cannot be negative.");
        return errors.ToArray();
    }
}
