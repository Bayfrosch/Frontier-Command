class_name SetRallyPointMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.SET_RALLY_POINT
var command_id: String
var issued_at_tick: int
var producer_entity_ids: PackedStringArray
var target: Variant


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_producer_entity_ids: PackedStringArray = PackedStringArray(),
		p_target: Variant = null,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	producer_entity_ids = p_producer_entity_ids
	target = p_target


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"producer_entity_ids": producer_entity_ids,
		"target": target,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	producer_entity_ids = payload.get(
		"producer_entity_ids",
		PackedStringArray(),
	)
	target = payload.get("target", null)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if producer_entity_ids.is_empty():
		errors.append("producer_entity_ids must contain at least one entity.")
	if target == null:
		return errors
	if not target is Dictionary:
		errors.append("target must be a Dictionary or null.")
		return errors

	var target_dictionary: Dictionary = target
	var kind: StringName = target_dictionary.get("kind", &"")
	if kind == &"position":
		if not target_dictionary.get("position", null) is Vector2:
			errors.append("target.position must be a Vector2.")
	elif kind == &"entity":
		var entity_id: String = target_dictionary.get("entity_id", "")
		if entity_id.is_empty():
			errors.append("target.entity_id is required.")
	else:
		errors.append("target.kind is invalid.")
	return errors
