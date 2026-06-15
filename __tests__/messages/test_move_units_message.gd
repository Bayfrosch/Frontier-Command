extends GutTest


func test_move_units_round_trip() -> void:
	var message := MoveUnitsMessage.new(
		"command-1",
		120,
		PackedStringArray(["unit-1", "unit-2"]),
		Vector2(100.0, 250.0),
		GameMessages.QueueMode.APPEND,
		&"rectangle",
	)

	assert_eq(message.message_type, GameMessages.MOVE_UNITS)
	assert_true(Interfaces.implements(message, &"MessageInterface"))
	assert_true(Interfaces.implements(message, &"CommandInterface"))
	assert_true(Interfaces.implements(message, &"QueueableCommandInterface"))
	assert_true(message.validate().is_empty())

	var restored := MoveUnitsMessage.new()
	restored.from_payload(message.to_payload())

	assert_eq(restored.command_id, message.command_id)
	assert_eq(restored.issued_at_tick, message.issued_at_tick)
	assert_eq(restored.queue_mode, message.queue_mode)
	assert_eq(restored.unit_ids, message.unit_ids)
	assert_eq(restored.destination, message.destination)
	assert_eq(restored.formation, message.formation)


func test_move_units_default_message_is_invalid() -> void:
	var message := MoveUnitsMessage.new()
	assert_eq(message.validate().size(), 3)

#
