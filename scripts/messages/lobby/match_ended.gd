class_name MatchEndedMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

const VALID_REASONS: Array[StringName] = [
	&"victory_conditions",
	&"surrender",
	&"all_opponents_disconnected",
	&"admin_ended",
]

var message_type: StringName = GameMessages.MATCH_ENDED
var end_tick: int
var winner_player_ids: PackedStringArray
var defeated_player_ids: PackedStringArray
var reason: StringName
var winner_team_id: String


func _init(
		p_end_tick: int = -1,
		p_winner_player_ids: PackedStringArray = PackedStringArray(),
		p_defeated_player_ids: PackedStringArray = PackedStringArray(),
		p_reason: StringName = &"",
		p_winner_team_id: String = "",
) -> void:
	end_tick = p_end_tick
	winner_player_ids = p_winner_player_ids
	defeated_player_ids = p_defeated_player_ids
	reason = p_reason
	winner_team_id = p_winner_team_id


func to_payload() -> Dictionary:
	var payload: Dictionary = {
		"end_tick": end_tick,
		"winner_player_ids": winner_player_ids,
		"defeated_player_ids": defeated_player_ids,
		"reason": reason,
	}
	if not winner_team_id.is_empty():
		payload["winner_team_id"] = winner_team_id
	return payload


func from_payload(payload: Dictionary) -> void:
	end_tick = payload.get("end_tick", -1)
	winner_player_ids = payload.get("winner_player_ids", PackedStringArray())
	defeated_player_ids = payload.get("defeated_player_ids", PackedStringArray())
	reason = payload.get("reason", &"")
	winner_team_id = payload.get("winner_team_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if end_tick < 0:
		errors.append("end_tick cannot be negative.")
	if reason not in VALID_REASONS:
		errors.append("reason is invalid.")
	return errors
