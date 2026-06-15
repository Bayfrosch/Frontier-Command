class_name GenericErrorMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]
const ERROR_CODES: Array[StringName] = [
	&"unsupported_protocol_version",
	&"authentication_failed",
	&"permission_denied",
	&"match_not_found",
	&"match_full",
	&"invalid_match_settings",
	&"player_not_ready",
	&"request_timeout",
	&"internal_server_error",
]

var message_type: StringName = GameMessages.GENERIC_ERROR
var code: StringName
var retryable: bool
var details: String


func _init(
		p_code: StringName = &"",
		p_retryable: bool = false,
		p_details: String = "",
) -> void:
	code = p_code
	retryable = p_retryable
	details = p_details


func to_payload() -> Dictionary:
	var payload := {
		"code": code,
		"retryable": retryable,
	}
	if not details.is_empty():
		payload["details"] = details
	return payload


func from_payload(payload: Dictionary) -> void:
	code = payload.get("code", &"")
	retryable = payload.get("retryable", false)
	details = payload.get("details", "")


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if code not in ERROR_CODES:
		errors.append("code is invalid.")
	return errors
