class_name JoinMatchMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.JOIN_MATCH
var match_id: String
var as_observer: bool


func _init(
		p_match_id: String = "",
		p_as_observer: bool = false,
) -> void:
	match_id = p_match_id
	as_observer = p_as_observer


func to_payload() -> Dictionary:
	return {
		"match_id": match_id,
		"as_observer": as_observer,
	}


func from_payload(payload: Dictionary) -> void:
	match_id = payload.get("match_id", "")
	as_observer = payload.get("as_observer", false)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if match_id.is_empty():
		errors.append("match_id is required.")
	return errors
