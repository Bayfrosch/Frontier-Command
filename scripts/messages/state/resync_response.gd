class_name ResyncResponseMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]

var message_type: StringName = GameMessages.RESYNC_RESPONSE
var snapshot: Dictionary


func _init(p_snapshot: Dictionary = {}) -> void:
	snapshot = p_snapshot


func to_payload() -> Dictionary:
	return {"snapshot": snapshot}


func from_payload(payload: Dictionary) -> void:
	snapshot = payload.get("snapshot", {})


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if snapshot.is_empty():
		errors.append("snapshot is required.")
	return errors
