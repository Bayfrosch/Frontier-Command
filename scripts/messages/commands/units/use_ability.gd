class_name UseAbilityMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

var message_type: StringName = GameMessages.USE_ABILITY
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var caster_entity_ids: PackedStringArray
var ability_id: String
var target_entity_id: String
var target_position: Variant


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_caster_entity_ids: PackedStringArray = PackedStringArray(),
		p_ability_id: String = "",
		p_queue_mode: int = GameMessages.QueueMode.REPLACE,
		p_target_entity_id: String = "",
		p_target_position: Variant = null,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	caster_entity_ids = p_caster_entity_ids
	ability_id = p_ability_id
	queue_mode = p_queue_mode
	target_entity_id = p_target_entity_id
	target_position = p_target_position


func to_payload() -> Dictionary:
	var payload := {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"caster_entity_ids": caster_entity_ids,
		"ability_id": ability_id,
	}
	if not target_entity_id.is_empty():
		payload["target_entity_id"] = target_entity_id
	if target_position != null:
		payload["target_position"] = target_position
	return payload


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get("queue_mode", GameMessages.QueueMode.REPLACE)
	caster_entity_ids = payload.get(
		"caster_entity_ids",
		PackedStringArray(),
	)
	ability_id = payload.get("ability_id", "")
	target_entity_id = payload.get("target_entity_id", "")
	target_position = payload.get("target_position", null)


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
	if caster_entity_ids.is_empty():
		errors.append("caster_entity_ids must contain at least one entity.")
	if ability_id.is_empty():
		errors.append("ability_id is required.")
	if not target_entity_id.is_empty() and target_position != null:
		errors.append("only one ability target may be supplied.")
	if target_position != null and not target_position is Vector2:
		errors.append("target_position must be a Vector2.")
	return errors
