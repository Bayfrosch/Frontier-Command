extends GutTest


func test_message_envelope_round_trip() -> void:
	var message := MessageEnvelope.new(
		GameMessages.MOVE_UNITS,
		{"command_id": "command-1"},
		"message-1",
		1000,
		"match-1",
		"player-1",
		"request-1",
	)
	var restored := MessageEnvelope.new()
	restored.from_payload(message.to_payload())

	assert_true(message.validate().is_empty())
	assert_eq(restored.to_payload(), message.to_payload())


func test_message_envelope_omits_optional_ids() -> void:
	var message := MessageEnvelope.new(
		GameMessages.PING,
		{},
		"message-1",
		1000,
	)

	assert_false(message.to_payload().has("match_id"))
	assert_false(message.to_payload().has("sender_player_id"))
	assert_false(message.to_payload().has("request_id"))


func test_command_result_accept_and_reject_round_trips() -> void:
	var accepted := CommandResultMessage.new(
		"command-1",
		&"accepted",
		121,
	)
	var rejected := CommandResultMessage.new(
		"command-2",
		&"rejected",
		-1,
		{
			"code": &"not_owner",
			"details": "The player does not own every selected unit.",
			"entity_ids": PackedStringArray(["unit-1"]),
		},
	)

	for message in [accepted, rejected]:
		var restored: RefCounted = message.get_script().new()
		restored.from_payload(message.to_payload())
		assert_true(message.validate().is_empty())
		assert_true(Interfaces.implements(message, &"MessageInterface"))
		assert_eq(restored.to_payload(), message.to_payload())


func test_state_sync_messages_round_trip() -> void:
	var snapshot := StateSnapshotMessage.new(
		"match-1",
		120,
		GameMessages.MatchPhase.RUNNING,
		6000,
		[{"player_id": "player-1"}],
		[{"entity_id": "unit-1"}],
		PackedByteArray([1, 2, 3]),
		"checksum-1",
	)
	var delta := StateDeltaMessage.new(
		120,
		121,
		[{"player_id": "player-1"}],
		[{"entity_id": "unit-1"}],
		PackedStringArray(["unit-2"]),
		"checksum-2",
		PackedByteArray([4, 5]),
	)
	var request := ResyncRequestMessage.new(
		121,
		"checksum-2",
		&"checksum_mismatch",
	)
	var response := ResyncResponseMessage.new(snapshot.to_payload())

	for message in [snapshot, delta, request, response]:
		var restored: RefCounted = message.get_script().new()
		restored.from_payload(message.to_payload())
		assert_true(message.validate().is_empty())
		assert_true(Interfaces.implements(message, &"MessageInterface"))
		assert_eq(restored.to_payload(), message.to_payload())


func test_state_delta_omits_empty_map_patch() -> void:
	var message := StateDeltaMessage.new(
		1,
		2,
		[],
		[],
		PackedStringArray(),
		"checksum",
	)

	assert_false(message.to_payload().has("explored_map_patch"))


func test_simulation_events_round_trip() -> void:
	var message := SimulationEventsMessage.new(
		121,
		[{
			"event_id": "event-1",
			"event_type": &"destination_unreachable",
			"tick": 121,
			"command_id": "command-1",
			"unit_entity_ids": PackedStringArray(["unit-1"]),
			"requested_destination": Vector2(100.0, 250.0),
		}],
	)
	var restored := SimulationEventsMessage.new()
	restored.from_payload(message.to_payload())

	assert_true(message.validate().is_empty())
	assert_eq(restored.to_payload(), message.to_payload())


func test_generic_error_round_trip_and_optional_details() -> void:
	var with_details := GenericErrorMessage.new(
		&"match_not_found",
		false,
		"No match exists for that id.",
	)
	var without_details := GenericErrorMessage.new(&"request_timeout", true)
	var restored := GenericErrorMessage.new()
	restored.from_payload(with_details.to_payload())

	assert_true(with_details.validate().is_empty())
	assert_true(without_details.validate().is_empty())
	assert_eq(restored.to_payload(), with_details.to_payload())
	assert_false(without_details.to_payload().has("details"))


func test_default_system_messages_are_invalid() -> void:
	assert_false(MessageEnvelope.new().validate().is_empty())
	assert_false(CommandResultMessage.new().validate().is_empty())
	assert_false(StateSnapshotMessage.new().validate().is_empty())
	assert_false(StateDeltaMessage.new().validate().is_empty())
	assert_false(ResyncRequestMessage.new().validate().is_empty())
	assert_false(ResyncResponseMessage.new().validate().is_empty())
	assert_false(SimulationEventsMessage.new().validate().is_empty())
	assert_false(GenericErrorMessage.new().validate().is_empty())


func test_invalid_system_message_values_are_rejected() -> void:
	assert_false(
		CommandResultMessage.new(
			"command-1",
			&"rejected",
			-1,
			{"code": &"not-a-code"},
		).validate().is_empty()
	)
	assert_false(
		ResyncRequestMessage.new(
			1,
			"checksum",
			&"not-a-reason",
		).validate().is_empty()
	)
	assert_false(
		SimulationEventsMessage.new(
			1,
			[{"event_type": &"unit_created", "tick": 1}],
		).validate().is_empty()
	)
	assert_false(GenericErrorMessage.new(&"not-a-code").validate().is_empty())
