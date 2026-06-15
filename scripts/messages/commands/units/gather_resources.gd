class_name GatherResourcesMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

var message_type: StringName = GameMessages.GATHER_RESOURCES
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var harvester_entity_ids: PackedStringArray
var resource_field_entity_id: String
var refinery_entity_id: String


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_harvester_entity_ids: PackedStringArray = PackedStringArray(),
		p_resource_field_entity_id: String = "",
		p_queue_mode: int = GameMessages.QueueMode.REPLACE,
		p_refinery_entity_id: String = "",
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	harvester_entity_ids = p_harvester_entity_ids
	resource_field_entity_id = p_resource_field_entity_id
	queue_mode = p_queue_mode
	refinery_entity_id = p_refinery_entity_id


func to_payload() -> Dictionary:
	var payload := {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"harvester_entity_ids": harvester_entity_ids,
		"resource_field_entity_id": resource_field_entity_id,
	}
	if not refinery_entity_id.is_empty():
		payload["refinery_entity_id"] = refinery_entity_id
	return payload


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get("queue_mode", GameMessages.QueueMode.REPLACE)
	harvester_entity_ids = payload.get(
		"harvester_entity_ids",
		PackedStringArray(),
	)
	resource_field_entity_id = payload.get("resource_field_entity_id", "")
	refinery_entity_id = payload.get("refinery_entity_id", "")


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
	if harvester_entity_ids.is_empty():
		errors.append(
			"harvester_entity_ids must contain at least one entity.",
		)
	if resource_field_entity_id.is_empty():
		errors.append("resource_field_entity_id is required.")
	return errors
