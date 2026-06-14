class_name ClientHelloMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.CLIENT_HELLO
var client_version: String
var player_name: String
var reconnect_token: String


func _init(
		p_client_version: String = "",
		p_player_name: String = "",
		p_reconnect_token: String = "",
) -> void:
	client_version = p_client_version
	player_name = p_player_name
	reconnect_token = p_reconnect_token


func to_payload() -> Dictionary:
	var payload: Dictionary = {
		"client_version": client_version,
		"player_name": player_name,
	}
	if not reconnect_token.is_empty():
		payload["reconnect_token"] = reconnect_token
	return payload


func from_payload(payload: Dictionary) -> void:
	client_version = payload.get("client_version", "")
	player_name = payload.get("player_name", "")
	reconnect_token = payload.get("reconnect_token", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if player_name.is_empty():
		errors.append("player_name is required.")
	if client_version.is_empty():
		errors.append("client_version is required.")
	return errors
