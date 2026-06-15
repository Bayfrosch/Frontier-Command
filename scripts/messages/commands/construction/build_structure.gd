class_name BuildStructureMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

var message_type: StringName = GameMessages.BUILD_STRUCTURE
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var construction_unit_id: String
var building_definition_id: String
var position: Vector2
var rotation_radians: float


func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_construction_unit_id: String = "",
		p_building_definition_id: String = "",
		p_position: Vector2 = Vector2.ZERO,
		p_rotation_radians: float = 0.0,
		p_queue_mode: int = GameMessages.QueueMode.REPLACE,
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	construction_unit_id = p_construction_unit_id
	building_definition_id = p_building_definition_id
	position = p_position
	rotation_radians = p_rotation_radians
	queue_mode = p_queue_mode


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"construction_unit_id": construction_unit_id,
		"building_definition_id": building_definition_id,
		"position": position,
		"rotation_radians": rotation_radians,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get("queue_mode", GameMessages.QueueMode.REPLACE)
	construction_unit_id = payload.get("construction_unit_id", "")
	building_definition_id = payload.get("building_definition_id", "")
	position = payload.get("position", Vector2.ZERO)
	rotation_radians = payload.get("rotation_radians", 0.0)


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
	if construction_unit_id.is_empty():
		errors.append("construction_unit_id is required.")
	if building_definition_id.is_empty():
		errors.append("building_definition_id is required.")
	return errors
