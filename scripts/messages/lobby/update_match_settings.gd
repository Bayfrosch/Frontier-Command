class_name UpdateMatchSettingsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]

var message_type: StringName = GameMessages.UPDATE_MATCH_SETTINGS
var settings: Dictionary


func _init(p_settings: Dictionary = {}) -> void:
	settings = p_settings


func to_payload() -> Dictionary:
	return {
		"settings": settings,
	}


func from_payload(payload: Dictionary) -> void:
	settings = payload.get("settings", {})


func validate() -> PackedStringArray:
	var match_settings := MatchSettingsMessage.new()
	match_settings.from_payload(settings)
	return match_settings.validate()
