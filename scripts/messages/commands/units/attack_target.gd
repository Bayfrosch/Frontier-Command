class_name AttackTargetMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
]

var message_type: StringName = GameMessages.ATTACK_TARGET
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var unit_ids: PackedStringArray
var target_id: String

func _init(
        p_command_id: String = "",
        p_issued_at_tick: int = -1,
        p_unit_ids: PackedStringArray = PackedStringArray(),
        p_target_id: String = "",
        p_queue_mode: int = GameMessages.QueueMode.REPLACE,
) -> void:
    command_id = p_command_id
    issued_at_tick = p_issued_at_tick
    unit_ids = p_unit_ids
    target_id = p_target_id
    queue_mode = p_queue_mode

func to_payload() -> Dictionary:
    return {
        "command_id": command_id,
        "issued_at_tick": issued_at_tick,
        "queue_mode": queue_mode,
        "unit_ids": unit_ids,
        "target_id": target_id,
    }

func from_payload(payload: Dictionary) -> void:
    command_id = payload.get("command_id", "")
    issued_at_tick = payload.get("issued_at_tick", -1)
    queue_mode = payload.get(
        "queue_mode",
        GameMessages.QueueMode.REPLACE,
    )
    unit_ids = payload.get("unit_ids", PackedStringArray())
    target_id = payload.get("target_id", "")