class_name CancelConstructionMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.CANCEL_CONSTRUCTION
var command_id: String
var issued_at_tick: int
var construction_site_id: String


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_construction_site_id: String = "",
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	construction_site_id = p_construction_site_id


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"construction_site_id": construction_site_id,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	construction_site_id = payload.get("construction_site_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if construction_site_id.is_empty():
		errors.append("construction_site_id is required.")
	return errors
