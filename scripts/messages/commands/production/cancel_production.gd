class_name CancelProductionMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.CANCEL_PRODUCTION
var command_id: String
var issued_at_tick: int
var producer_entity_id: String
var queue_item_id: String


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_producer_entity_id: String = "",
		p_queue_item_id: String = "",
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	producer_entity_id = p_producer_entity_id
	queue_item_id = p_queue_item_id


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"producer_entity_id": producer_entity_id,
		"queue_item_id": queue_item_id,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	producer_entity_id = payload.get("producer_entity_id", "")
	queue_item_id = payload.get("queue_item_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if producer_entity_id.is_empty():
		errors.append("producer_entity_id is required.")
	if queue_item_id.is_empty():
		errors.append("queue_item_id is required.")
	return errors
