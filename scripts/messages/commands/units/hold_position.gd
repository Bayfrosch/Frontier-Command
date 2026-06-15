class_name HoldPositionMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.HOLD_POSITION_UNITS
var command_id: String
var issued_at_tick: int
var unit_ids: PackedStringArray
var enabled: bool = true


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_unit_ids: PackedStringArray = PackedStringArray(),
		p_enabled: bool = true,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	unit_ids = p_unit_ids
	enabled = p_enabled


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"unit_ids": unit_ids,
		"enabled": enabled,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	unit_ids = payload.get("unit_ids", PackedStringArray())
	enabled = payload.get("enabled", true)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if unit_ids.is_empty():
		errors.append("unit_ids must contain at least one unit.")
	return errors
