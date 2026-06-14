class_name PauseMatchMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.PAUSE_MATCH


func to_payload() -> Dictionary:
	return {}


func from_payload(_payload: Dictionary) -> void:
	pass


func validate() -> PackedStringArray:
	return PackedStringArray()
