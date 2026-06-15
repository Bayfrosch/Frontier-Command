class_name ResyncRequestMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]
const VALID_REASONS: Array[StringName] = [
	&"missing_delta",
	&"checksum_mismatch",
	&"reconnect",
]

var message_type: StringName = GameMessages.RESYNC_REQUEST
var client_tick: int
var client_checksum: String
var reason: StringName


func _init(
		p_client_tick: int = -1,
		p_client_checksum: String = "",
		p_reason: StringName = &"",
) -> void:
	client_tick = p_client_tick
	client_checksum = p_client_checksum
	reason = p_reason


func to_payload() -> Dictionary:
	return {
		"client_tick": client_tick,
		"client_checksum": client_checksum,
		"reason": reason,
	}


func from_payload(payload: Dictionary) -> void:
	client_tick = payload.get("client_tick", -1)
	client_checksum = payload.get("client_checksum", "")
	reason = payload.get("reason", &"")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if client_tick < 0:
		errors.append("client_tick cannot be negative.")
	if client_checksum.is_empty():
		errors.append("client_checksum is required.")
	if reason not in VALID_REASONS:
		errors.append("reason is invalid.")
	return errors
