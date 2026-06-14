class_name MatchStartedMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.MATCH_STARTED
var match_id: String
var start_tick: int
var tick_rate: int
var map_seed: String
var local_player_id: String


func _init(
		p_match_id: String = "",
		p_start_tick: int = -1,
		p_tick_rate: int = 0,
		p_map_seed: String = "",
		p_local_player_id: String = "",
) -> void:
	match_id = p_match_id
	start_tick = p_start_tick
	tick_rate = p_tick_rate
	map_seed = p_map_seed
	local_player_id = p_local_player_id


func to_payload() -> Dictionary:
	var payload: Dictionary = {
		"match_id": match_id,
		"start_tick": start_tick,
		"tick_rate": tick_rate,
		"map_seed": map_seed,
	}
	if not local_player_id.is_empty():
		payload["local_player_id"] = local_player_id
	return payload


func from_payload(payload: Dictionary) -> void:
	match_id = payload.get("match_id", "")
	start_tick = payload.get("start_tick", -1)
	tick_rate = payload.get("tick_rate", 0)
	map_seed = payload.get("map_seed", "")
	local_player_id = payload.get("local_player_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if match_id.is_empty():
		errors.append("match_id is required.")
	if start_tick < 0:
		errors.append("start_tick cannot be negative.")
	if tick_rate <= 0:
		errors.append("tick_rate must be positive.")
	if map_seed.is_empty():
		errors.append("map_seed is required.")
	return errors
