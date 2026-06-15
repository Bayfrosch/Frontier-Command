class_name MoveUnitsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

# Pre defined type of message
var message_type: StringName = GameMessages.MOVE_UNITS
# Unique id to prevent multiple execution of same command
var command_id: String
# Tracks the tick the command was issued by Player or AI
var issued_at_tick: int
# Mode which the command accesses the Queue
var queue_mode: int = GameMessages.QueueMode.REPLACE
var unit_ids: PackedStringArray
var destination: Vector2
# Formation in which units end in
var formation: StringName = &"rectangle"

func _init(
		p_command_id: String = "",
		p_issued_at_tick: int = -1,
		p_unit_ids: PackedStringArray = PackedStringArray(),
		p_destination: Vector2 = Vector2.ZERO,
		p_queue_mode: int = GameMessages.QueueMode.REPLACE,
		p_formation: StringName = &"rectangle",
) -> void:
	command_id = p_command_id
	issued_at_tick = p_issued_at_tick
	unit_ids = p_unit_ids
	destination = p_destination
	queue_mode = p_queue_mode
	formation = p_formation


func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"unit_ids": unit_ids,
		"destination": destination,
		"formation": formation,
	}


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get(
		"queue_mode",
		GameMessages.QueueMode.REPLACE,
	)
	unit_ids = payload.get("unit_ids", PackedStringArray())
	destination = payload.get("destination", Vector2.ZERO)
	formation = payload.get("formation", &"rectangle")


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
	if unit_ids.is_empty():
		errors.append("unit_ids must contain at least one unit.")
	if formation not in [&"none", &"rectangle"]:
		errors.append("formation is invalid.")
	return errors

#
