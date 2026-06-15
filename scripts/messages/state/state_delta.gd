class_name StateDeltaMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]

var message_type: StringName = GameMessages.STATE_DELTA
var from_tick: int
var to_tick: int
var changed_players: Array[Dictionary]
var upserted_entities: Array[Dictionary]
var removed_entity_ids: PackedStringArray
var checksum: String
var explored_map_patch: PackedByteArray


func _init(
		p_from_tick: int = -1,
		p_to_tick: int = -1,
		p_changed_players: Array[Dictionary] = [],
		p_upserted_entities: Array[Dictionary] = [],
		p_removed_entity_ids: PackedStringArray = PackedStringArray(),
		p_checksum: String = "",
		p_explored_map_patch: PackedByteArray = PackedByteArray(),
) -> void:
	from_tick = p_from_tick
	to_tick = p_to_tick
	changed_players = p_changed_players
	upserted_entities = p_upserted_entities
	removed_entity_ids = p_removed_entity_ids
	checksum = p_checksum
	explored_map_patch = p_explored_map_patch


func to_payload() -> Dictionary:
	var payload := {
		"from_tick": from_tick,
		"to_tick": to_tick,
		"changed_players": changed_players,
		"upserted_entities": upserted_entities,
		"removed_entity_ids": removed_entity_ids,
		"checksum": checksum,
	}
	if not explored_map_patch.is_empty():
		payload["explored_map_patch"] = explored_map_patch
	return payload


func from_payload(payload: Dictionary) -> void:
	from_tick = payload.get("from_tick", -1)
	to_tick = payload.get("to_tick", -1)
	changed_players = payload.get("changed_players", [])
	upserted_entities = payload.get("upserted_entities", [])
	removed_entity_ids = payload.get("removed_entity_ids", PackedStringArray())
	checksum = payload.get("checksum", "")
	explored_map_patch = payload.get("explored_map_patch", PackedByteArray())


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if from_tick < 0:
		errors.append("from_tick cannot be negative.")
	if to_tick < from_tick:
		errors.append("to_tick cannot be earlier than from_tick.")
	if checksum.is_empty():
		errors.append("checksum is required.")
	return errors
