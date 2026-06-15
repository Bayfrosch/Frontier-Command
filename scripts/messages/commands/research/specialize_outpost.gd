class_name SpecializeOutpostMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.SPECIALIZE_OUTPOST
var command_id: String
var issued_at_tick: int
var outpost_entity_id: String
var specialization: int = GameMessages.OutpostSpecialization.INDUSTRIAL


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_outpost_entity_id: String = "",
		p_specialization: int = GameMessages.OutpostSpecialization.INDUSTRIAL,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	outpost_entity_id = p_outpost_entity_id
	specialization = p_specialization


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"outpost_entity_id": outpost_entity_id,
		"specialization": specialization,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	outpost_entity_id = payload.get("outpost_entity_id", "")
	specialization = payload.get(
		"specialization",
		GameMessages.OutpostSpecialization.INDUSTRIAL,
	)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if outpost_entity_id.is_empty():
		errors.append("outpost_entity_id is required.")
	if specialization not in [
		GameMessages.OutpostSpecialization.INDUSTRIAL,
		GameMessages.OutpostSpecialization.MILITARY,
		GameMessages.OutpostSpecialization.RESEARCH,
	]:
		errors.append("specialization is invalid.")
	return errors
