class_name TrainUnitsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.TRAIN_UNITS
var command_id: String
var issued_at_tick: int
var producer_entity_id: String
var unit_definition_id: String
var quantity: int = 1


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_producer_entity_id: String = "",
		p_unit_definition_id: String = "",
		p_quantity: int = 1,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	producer_entity_id = p_producer_entity_id
	unit_definition_id = p_unit_definition_id
	quantity = p_quantity


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"producer_entity_id": producer_entity_id,
		"unit_definition_id": unit_definition_id,
		"quantity": quantity,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	producer_entity_id = payload.get("producer_entity_id", "")
	unit_definition_id = payload.get("unit_definition_id", "")
	quantity = payload.get("quantity", 1)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if producer_entity_id.is_empty():
		errors.append("producer_entity_id is required.")
	if unit_definition_id.is_empty():
		errors.append("unit_definition_id is required.")
	if quantity <= 0:
		errors.append("quantity must be greater than zero.")
	return errors
