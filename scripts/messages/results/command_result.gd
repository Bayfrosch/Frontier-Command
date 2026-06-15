class_name CommandResultMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]

const VALID_STATUSES: Array[StringName] = [&"accepted", &"rejected"]
const REJECTION_CODES: Array[StringName] = [
	&"malformed_command",
	&"player_not_in_match",
	&"player_defeated",
	&"not_owner",
	&"entity_not_found",
	&"entity_destroyed",
	&"invalid_target",
	&"target_not_visible",
	&"unsupported_command",
	&"invalid_placement",
	&"unreachable_destination",
	&"insufficient_materials",
	&"insufficient_energy",
	&"fleet_capacity_exceeded",
	&"missing_prerequisite",
	&"research_locked",
	&"already_researched",
	&"doctrine_already_chosen",
	&"queue_full",
	&"match_not_running",
	&"rate_limited",
]

var message_type: StringName = GameMessages.COMMAND_RESULT
var command_id: String
var status: StringName
var accepted_at_tick: int = -1
var rejection: Dictionary


func _init(
		p_command_id: String = "",
		p_status: StringName = &"",
		p_accepted_at_tick: int = -1,
		p_rejection: Dictionary = {},
) -> void:
	command_id = p_command_id
	status = p_status
	accepted_at_tick = p_accepted_at_tick
	rejection = p_rejection


func to_payload() -> Dictionary:
	var payload := {
		"command_id": command_id,
		"status": status,
	}
	if status == &"accepted":
		payload["accepted_at_tick"] = accepted_at_tick
	elif status == &"rejected":
		payload["rejection"] = rejection
	return payload


func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	status = payload.get("status", &"")
	accepted_at_tick = payload.get("accepted_at_tick", -1)
	rejection = payload.get("rejection", {})


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if status not in VALID_STATUSES:
		errors.append("status is invalid.")
	if status == &"accepted" and accepted_at_tick < 0:
		errors.append("accepted_at_tick cannot be negative.")
	if status == &"rejected":
		if rejection.is_empty():
			errors.append("rejection is required.")
		elif rejection.get("code", &"") not in REJECTION_CODES:
			errors.append("rejection.code is invalid.")
	return errors
