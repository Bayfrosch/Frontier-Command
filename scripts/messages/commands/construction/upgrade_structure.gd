class_name UpgradeStructureMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
]

var message_type: StringName = GameMessages.UPGRADE_STRUCTURE
var command_id: String
var issued_at_tick: int
var structure_entity_id: String
var upgrade_definition_id: String


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_structure_entity_id: String = "",
		p_upgrade_definition_id: String = "",
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	structure_entity_id = p_structure_entity_id
	upgrade_definition_id = p_upgrade_definition_id


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"structure_entity_id": structure_entity_id,
		"upgrade_definition_id": upgrade_definition_id,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	structure_entity_id = payload.get("structure_entity_id", "")
	upgrade_definition_id = payload.get("upgrade_definition_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if structure_entity_id.is_empty():
		errors.append("structure_entity_id is required.")
	if upgrade_definition_id.is_empty():
		errors.append("upgrade_definition_id is required.")
	return errors
