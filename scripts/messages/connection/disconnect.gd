class_name DisconnectMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

const VALID_REASONS: Array[StringName] = [
	&"client_quit",
	&"timeout",
	&"kicked",
	&"server_shutdown",
	&"protocol_error",
]

var message_type: StringName = GameMessages.DISCONNECT
var reason: StringName


func _init(p_reason: StringName = &"") -> void:
	reason = p_reason


func to_payload() -> Dictionary:
	return {
		"reason": reason,
	}


func from_payload(payload: Dictionary) -> void:
	reason = payload.get("reason", &"")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if reason not in VALID_REASONS:
		errors.append("reason is invalid.")
	return errors
