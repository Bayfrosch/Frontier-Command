extends SceneTree


func _initialize() -> void:
	var message := MoveUnitsMessage.new(
		"command-1",
		120,
		PackedStringArray(["unit-1", "unit-2"]),
		Vector2(100.0, 250.0),
		GameMessages.QueueMode.APPEND,
		&"rectangle",
	)

	assert(message.message_type == GameMessages.MOVE_UNITS)
	assert(Interfaces.implements(message, &"MessageInterface"))
	assert(Interfaces.implements(message, &"CommandInterface"))
	assert(Interfaces.implements(message, &"QueueableCommandInterface"))
	assert(message.validate().is_empty())

	var payload := message.to_payload()
	var restored := MoveUnitsMessage.new()
	restored.from_payload(payload)

	assert(restored.command_id == message.command_id)
	assert(restored.issued_at_tick == message.issued_at_tick)
	assert(restored.queue_mode == message.queue_mode)
	assert(restored.unit_ids == message.unit_ids)
	assert(restored.destination == message.destination)
	assert(restored.formation == message.formation)

	var invalid := MoveUnitsMessage.new()
	assert(invalid.validate().size() == 3)

	print("MoveUnitsMessage tests passed.")
	quit()
