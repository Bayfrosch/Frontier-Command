class_name StartMatchMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.START_MATCH


func to_payload() -> Dictionary:
	return {}


func from_payload(_payload: Dictionary) -> void:
	pass


func validate() -> PackedStringArray:
	return PackedStringArray()
