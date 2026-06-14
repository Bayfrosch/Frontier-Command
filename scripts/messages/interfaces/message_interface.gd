class_name MessageInterface
extends BasicInterface

var message_type: StringName

func to_payload() -> Dictionary:
	return {}

func from_payload(_payload: Dictionary) -> void:
	pass

func validate() -> PackedStringArray:
	return PackedStringArray()
