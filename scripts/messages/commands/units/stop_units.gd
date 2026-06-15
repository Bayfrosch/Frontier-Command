class_name StopUnitsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
    &"MessageInterface",
    &"CommandInterface",
]

var message_type: StringName = GameMessages.STOP_UNITS
var command_id: String
var issued_at_tick: int
var unit_ids: PackedStringArray

func _init(
        p_command_id: String = "",
        p_issued_at_tick: int = -1,
        p_unit_ids: PackedStringArray = PackedStringArray(),
) -> void:
    command_id = p_command_id
    issued_at_tick = p_issued_at_tick
    unit_ids = p_unit_ids

func to_payload() -> Dictionary:
    return {
        "command_id": command_id,
        "issued_at_tick": issued_at_tick,
        "unit_ids": unit_ids,
    }

func from_payload(payload: Dictionary) -> void:
    command_id = payload.get("command_id", "")
    issued_at_tick = payload.get("issued_at_tick", -1)
    unit_ids = payload.get("unit_ids", PackedStringArray())
