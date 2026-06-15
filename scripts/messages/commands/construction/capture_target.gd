class_name CaptureTargetMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

var message_type: StringName = GameMessages.CAPTURE_TARGET
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var envoy_unit_ids: PackedStringArray
var target_entity_id: String


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_envoy_unit_ids: PackedStringArray = PackedStringArray(),
		p_target_entity_id: String = "",
		p_queue_mode: int = GameMessages.QueueMode.REPLACE,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	envoy_unit_ids = p_envoy_unit_ids
	target_entity_id = p_target_entity_id
	queue_mode = p_queue_mode


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"envoy_unit_ids": envoy_unit_ids,
		"target_entity_id": target_entity_id,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get("queue_mode", GameMessages.QueueMode.REPLACE)
	envoy_unit_ids = payload.get("envoy_unit_ids", PackedStringArray())
	target_entity_id = payload.get("target_entity_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if queue_mode not in [
		GameMessages.QueueMode.REPLACE,
		GameMessages.QueueMode.APPEND,
	]:
		errors.append("queue_mode is invalid.")
	if envoy_unit_ids.is_empty():
		errors.append("envoy_unit_ids must contain at least one unit.")
	if target_entity_id.is_empty():
		errors.append("target_entity_id is required.")
	return errors
