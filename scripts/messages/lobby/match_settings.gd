class_name MatchSettingsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.MATCH_SETTINGS
var match_name: String
var max_players: int
var map_seed: String
var map_template: String
var map_width_tiles: int
var map_height_tiles: int
var allow_pause: bool


func _init(
		p_match_name: String = "",
		p_max_players: int = 2,
		p_map_seed: String = "seed",
		p_map_template: String = "standard_1v1",
		p_map_width_tiles: int = 1,
		p_map_height_tiles: int = 1,
		p_allow_pause: bool = true,
) -> void:
	match_name = p_match_name
	max_players = p_max_players
	map_seed = p_map_seed
	map_template = p_map_template
	map_width_tiles = p_map_width_tiles
	map_height_tiles = p_map_height_tiles
	allow_pause = p_allow_pause


func to_payload() -> Dictionary:
	return {
		"match_name": match_name,
		"max_players": max_players,
		"map_seed": map_seed,
		"map_template": map_template,
		"map_width_tiles": map_width_tiles,
		"map_height_tiles": map_height_tiles,
		"allow_pause": allow_pause,
	}


func from_payload(payload: Dictionary) -> void:
	match_name = payload.get("match_name", "")
	max_players = payload.get("max_players", 2)
	map_seed = payload.get("map_seed", "seed")
	map_template = payload.get("map_template", "standard_1v1")
	map_width_tiles = payload.get("map_width_tiles", 1)
	map_height_tiles = payload.get("map_height_tiles", 1)
	allow_pause = payload.get("allow_pause", true)


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if match_name.is_empty():
		errors.append("match_name is empty")
	if max_players <= 0:
		errors.append("max_players must be greater than 0")
	if map_seed.is_empty():
		errors.append("map_seed is empty")
	if map_template.is_empty():
		errors.append("map_template is empty")
	if map_width_tiles <= 0:
		errors.append("map_width_tiles must be greater than 0")
	if map_height_tiles <= 0:
		errors.append("map_height_tiles must be greater than 0")
	return errors
