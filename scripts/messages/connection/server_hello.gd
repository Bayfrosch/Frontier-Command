class_name ServerHelloMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.SERVER_HELLO
var server_version: String
var protocol_version: int
var player_id: String
var reconnect_token: String
var server_time_unix_ms: int


func _init(
		p_server_version: String = "",
		p_protocol_version: int = -1,
		p_player_id: String = "",
		p_reconnect_token: String = "",
		p_server_time_unix_ms: int = -1,
) -> void:
	server_version = p_server_version
	protocol_version = p_protocol_version
	player_id = p_player_id
	reconnect_token = p_reconnect_token
	server_time_unix_ms = p_server_time_unix_ms


func to_payload() -> Dictionary:
	return {
		"server_version": server_version,
		"protocol_version": protocol_version,
		"player_id": player_id,
		"reconnect_token": reconnect_token,
		"server_time_unix_ms": server_time_unix_ms,
	}


func from_payload(payload: Dictionary) -> void:
	server_version = payload.get("server_version", "")
	protocol_version = payload.get("protocol_version", -1)
	player_id = payload.get("player_id", "")
	reconnect_token = payload.get("reconnect_token", "")
	server_time_unix_ms = payload.get("server_time_unix_ms", -1)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if server_version.is_empty():
		errors.append("server_version is required.")
	if protocol_version < 1:
		errors.append("protocol_version must be positive.")
	if player_id.is_empty():
		errors.append("player_id is required.")
	if reconnect_token.is_empty():
		errors.append("reconnect_token is required.")
	if server_time_unix_ms < 0:
		errors.append("server_time_unix_ms cannot be negative.")
	return errors
