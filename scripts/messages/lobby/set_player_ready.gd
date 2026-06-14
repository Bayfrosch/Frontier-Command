class_name SetPlayerReadyMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.SET_PLAYER_READY
var is_ready: bool
var faction_id: String
var team_id: String


func _init(
		p_is_ready: bool = false,
		p_faction_id: String = "",
		p_team_id: String = "",
) -> void:
	is_ready = p_is_ready
	faction_id = p_faction_id
	team_id = p_team_id


func to_payload() -> Dictionary:
	var payload: Dictionary = {
		"is_ready": is_ready,
		"faction_id": faction_id,
	}
	if not team_id.is_empty():
		payload["team_id"] = team_id
	return payload


func from_payload(payload: Dictionary) -> void:
	is_ready = payload.get("is_ready", false)
	faction_id = payload.get("faction_id", "")
	team_id = payload.get("team_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if faction_id.is_empty():
		errors.append("faction_id is required.")
	return errors
