class_name LobbyStateMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.LOBBY_STATE
var lobby_id: String
var host_player_id: String
var settings: Dictionary
# Player dictionary contains: player_id, display_name, is_ai, is_ready, team_id, faction, color
var players: Array[Dictionary]


func _init(
		p_lobby_id: String = "",
		p_host_player_id: String = "",
		p_settings: Dictionary = {},
		p_players: Array[Dictionary] = [],
) -> void:
	lobby_id = p_lobby_id
	host_player_id = p_host_player_id
	settings = p_settings
	players = p_players


func to_payload() -> Dictionary:
	return {
		"lobby_id": lobby_id,
		"host_player_id": host_player_id,
		"settings": settings,
		"players": players,
	}


func from_payload(payload: Dictionary) -> void:
	lobby_id = payload.get("lobby_id", "")
	host_player_id = payload.get("host_player_id", "")
	settings = payload.get("settings", {})
	players = payload.get("players", [])


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if lobby_id.is_empty():
		errors.append("lobby_id is empty")
	if host_player_id.is_empty():
		errors.append("host_player_id is empty")
	if not settings.has("match_name") or settings["match_name"].is_empty():
		errors.append("match_name is missing or empty in settings")
	if not settings.has("max_players") or settings["max_players"] <= 0:
		errors.append("max_players is missing or invalid in settings")
	if not settings.has("map_seed") or settings["map_seed"].is_empty():
		errors.append("map_seed is missing or empty in settings")
	if not settings.has("map_template") or settings["map_template"].is_empty():
		errors.append("map_template is missing or empty in settings")
	if not settings.has("map_width_tiles") or settings["map_width_tiles"] <= 0:
		errors.append("map_width_tiles is missing or invalid in settings")
	if not settings.has("map_height_tiles") or settings["map_height_tiles"] <= 0:
		errors.append("map_height_tiles is missing or invalid in settings")
	if not settings.has("allow_pause") or typeof(settings["allow_pause"]) != TYPE_BOOL:
		errors.append("allow_pause is missing or invalid in settings")
	for player in players:
		if not player.has("player_id") or player["player_id"].is_empty():
			errors.append("player_id is missing or empty for a player in players")
		if not player.has("display_name") or player["display_name"].is_empty():
			errors.append("display_name is missing or empty for a player in players")
		if not player.has("is_ai") or typeof(player["is_ai"]) != TYPE_BOOL:
			errors.append("is_ai is missing or invalid for a player in players")
		if not player.has("is_ready") or typeof(player["is_ready"]) != TYPE_BOOL:
			errors.append("is_ready is missing or invalid for a player in players")
		if not player.has("team_id") or player["team_id"] < 0:
			errors.append("team_id is missing or invalid for a player in players")
		if not player.has("faction") or player["faction"].is_empty():
			errors.append("faction is missing or empty for a player in players")
		if not player.has("color") or typeof(player["color"]) != TYPE_COLOR:
			errors.append("color is missing or invalid for a player in players")
	if not players:
		errors.append("players array is empty")
	return errors
