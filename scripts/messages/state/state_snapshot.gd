class_name StateSnapshotMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]

var message_type: StringName = GameMessages.STATE_SNAPSHOT
var match_id: String
var tick: int
var phase: int = GameMessages.MatchPhase.LOBBY
var match_time_ms: int
var players: Array[Dictionary]
var entities: Array[Dictionary]
var explored_map_data: PackedByteArray
var checksum: String


func _init(
		p_match_id: String = "",
		p_tick: int = -1,
		p_phase: int = GameMessages.MatchPhase.LOBBY,
		p_match_time_ms: int = -1,
		p_players: Array[Dictionary] = [],
		p_entities: Array[Dictionary] = [],
		p_explored_map_data: PackedByteArray = PackedByteArray(),
		p_checksum: String = "",
) -> void:
	match_id = p_match_id
	tick = p_tick
	phase = p_phase
	match_time_ms = p_match_time_ms
	players = p_players
	entities = p_entities
	explored_map_data = p_explored_map_data
	checksum = p_checksum


func to_payload() -> Dictionary:
	return {
		"match_id": match_id,
		"tick": tick,
		"phase": phase,
		"match_time_ms": match_time_ms,
		"players": players,
		"entities": entities,
		"explored_map_data": explored_map_data,
		"checksum": checksum,
	}


func from_payload(payload: Dictionary) -> void:
	match_id = payload.get("match_id", "")
	tick = payload.get("tick", -1)
	phase = payload.get("phase", GameMessages.MatchPhase.LOBBY)
	match_time_ms = payload.get("match_time_ms", -1)
	players = payload.get("players", [])
	entities = payload.get("entities", [])
	explored_map_data = payload.get("explored_map_data", PackedByteArray())
	checksum = payload.get("checksum", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if match_id.is_empty():
		errors.append("match_id is required.")
	if tick < 0:
		errors.append("tick cannot be negative.")
	if phase not in [
		GameMessages.MatchPhase.LOBBY,
		GameMessages.MatchPhase.RUNNING,
		GameMessages.MatchPhase.PAUSED,
		GameMessages.MatchPhase.ENDED,
	]:
		errors.append("phase is invalid.")
	if match_time_ms < 0:
		errors.append("match_time_ms cannot be negative.")
	if checksum.is_empty():
		errors.append("checksum is required.")
	return errors
