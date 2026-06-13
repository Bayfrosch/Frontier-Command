class_name PingMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.PING
var client_time_unix_ms: int


func _init(p_client_time_unix_ms: int = -1) -> void:
	client_time_unix_ms = p_client_time_unix_ms


func to_payload() -> Dictionary:
	return {
		"client_time_unix_ms": client_time_unix_ms,
	}


func from_payload(payload: Dictionary) -> void:
	client_time_unix_ms = payload.get("client_time_unix_ms", -1)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if client_time_unix_ms < 0:
		errors.append("client_time_unix_ms cannot be negative.")
	return errors
