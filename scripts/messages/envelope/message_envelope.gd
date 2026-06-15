class_name MessageEnvelope
extends RefCounted

const PROTOCOL_VERSION: int = 1

var protocol_version: int = PROTOCOL_VERSION
var message_id: String
var message_type: StringName
var sent_at_unix_ms: int
var payload: Dictionary
var match_id: String
var sender_player_id: String
var request_id: String


func _init(
		p_message_type: StringName = &"",
		p_payload: Dictionary = {},
		p_message_id: String = "",
		p_sent_at_unix_ms: int = -1,
		p_match_id: String = "",
		p_sender_player_id: String = "",
		p_request_id: String = "",
) -> void:
	message_type = p_message_type
	payload = p_payload
	message_id = p_message_id
	sent_at_unix_ms = p_sent_at_unix_ms
	match_id = p_match_id
	sender_player_id = p_sender_player_id
	request_id = p_request_id


func to_payload() -> Dictionary:
	var envelope := {
		"protocol_version": protocol_version,
		"message_id": message_id,
		"message_type": message_type,
		"sent_at_unix_ms": sent_at_unix_ms,
		"payload": payload,
	}
	if not match_id.is_empty():
		envelope["match_id"] = match_id
	if not sender_player_id.is_empty():
		envelope["sender_player_id"] = sender_player_id
	if not request_id.is_empty():
		envelope["request_id"] = request_id
	return envelope


func from_payload(envelope: Dictionary) -> void:
	protocol_version = envelope.get("protocol_version", PROTOCOL_VERSION)
	message_id = envelope.get("message_id", "")
	message_type = envelope.get("message_type", &"")
	sent_at_unix_ms = envelope.get("sent_at_unix_ms", -1)
	payload = envelope.get("payload", {})
	match_id = envelope.get("match_id", "")
	sender_player_id = envelope.get("sender_player_id", "")
	request_id = envelope.get("request_id", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if protocol_version != PROTOCOL_VERSION:
		errors.append("protocol_version is unsupported.")
	if message_id.is_empty():
		errors.append("message_id is required.")
	if message_type == &"":
		errors.append("message_type is required.")
	if sent_at_unix_ms < 0:
		errors.append("sent_at_unix_ms cannot be negative.")
	return errors
