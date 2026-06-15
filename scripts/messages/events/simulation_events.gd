class_name SimulationEventsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [&"MessageInterface"]

var message_type: StringName = GameMessages.SIMULATION_EVENTS
var tick: int
var events: Array[Dictionary]


func _init(
		p_tick: int = -1,
		p_events: Array[Dictionary] = [],
) -> void:
	tick = p_tick
	events = p_events


func to_payload() -> Dictionary:
	return {
		"tick": tick,
		"events": events,
	}


func from_payload(payload: Dictionary) -> void:
	tick = payload.get("tick", -1)
	events = payload.get("events", [])


func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if tick < 0:
		errors.append("tick cannot be negative.")
	for event in events:
		if not event.has("event_id") or str(event.get("event_id", "")).is_empty():
			errors.append("event_id is required.")
		if not event.has("event_type") or event.get("event_type", &"") == &"":
			errors.append("event_type is required.")
		if event.get("tick", -1) < 0:
			errors.append("event.tick cannot be negative.")
	return errors
