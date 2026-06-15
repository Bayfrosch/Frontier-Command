extends GutTest


func test_construction_command_round_trips() -> void:
	var messages := [
		BuildStructureMessage.new(
			"command-1",
			120,
			"worker-1",
			"basic_generator",
			Vector2(100.0, 250.0),
			0.5,
			GameMessages.QueueMode.APPEND,
		),
		CancelConstructionMessage.new("command-2", 121, "site-1"),
		RepairTargetMessage.new(
			"command-3",
			122,
			PackedStringArray(["worker-1"]),
			"building-1",
			GameMessages.QueueMode.APPEND,
		),
		CaptureTargetMessage.new(
			"command-4",
			123,
			PackedStringArray(["envoy-1"]),
			"outpost-1",
			GameMessages.QueueMode.APPEND,
		),
		UpgradeStructureMessage.new(
			"command-5",
			124,
			"building-1",
			"advanced_production",
		),
		CancelStructureUpgradeMessage.new(
			"command-6",
			125,
			"building-1",
			"queue-item-1",
		),
	]

	for message in messages:
		assert_true(message.validate().is_empty())
		assert_true(Interfaces.implements(message, &"MessageInterface"))
		assert_true(Interfaces.implements(message, &"CommandInterface"))
		var restored: RefCounted = message.get_script().new()
		restored.from_payload(message.to_payload())
		assert_eq(restored.to_payload(), message.to_payload())

	assert_true(Interfaces.implements(messages[0], &"QueueableCommandInterface"))
	assert_true(Interfaces.implements(messages[2], &"QueueableCommandInterface"))
	assert_true(Interfaces.implements(messages[3], &"QueueableCommandInterface"))


func test_production_command_round_trips() -> void:
	var messages := [
		TrainUnitsMessage.new(
			"command-7",
			126,
			"factory-1",
			"main_battle_unit",
			3,
		),
		CancelProductionMessage.new(
			"command-8",
			127,
			"factory-1",
			"queue-item-1",
		),
		ReorderProductionMessage.new(
			"command-9",
			128,
			"factory-1",
			"queue-item-1",
			0,
		),
	]

	for message in messages:
		assert_true(message.validate().is_empty())
		assert_true(Interfaces.implements(message, &"MessageInterface"))
		assert_true(Interfaces.implements(message, &"CommandInterface"))
		var restored: RefCounted = message.get_script().new()
		restored.from_payload(message.to_payload())
		assert_eq(restored.to_payload(), message.to_payload())


func test_set_rally_point_supports_target_variants() -> void:
	var position_target := {
		"kind": &"position",
		"position": Vector2(100.0, 250.0),
	}
	var entity_target := {
		"kind": &"entity",
		"entity_id": "entity-1",
	}
	var messages := [
		SetRallyPointMessage.new(
			"command-10",
			129,
			PackedStringArray(["factory-1"]),
			position_target,
		),
		SetRallyPointMessage.new(
			"command-11",
			130,
			PackedStringArray(["factory-1"]),
			entity_target,
		),
		SetRallyPointMessage.new(
			"command-12",
			131,
			PackedStringArray(["factory-1"]),
			null,
		),
	]

	for message in messages:
		assert_true(message.validate().is_empty())
		var restored := SetRallyPointMessage.new()
		restored.from_payload(message.to_payload())
		assert_eq(restored.to_payload(), message.to_payload())


func test_research_command_round_trips() -> void:
	var messages := [
		StartResearchMessage.new(
			"command-13",
			132,
			"research-center-1",
			"research-1",
		),
		CancelResearchMessage.new(
			"command-14",
			133,
			"research-center-1",
			"queue-item-1",
		),
		SpecializeOutpostMessage.new(
			"command-15",
			134,
			"outpost-1",
			GameMessages.OutpostSpecialization.RESEARCH,
		),
	]

	for message in messages:
		assert_true(message.validate().is_empty())
		assert_true(Interfaces.implements(message, &"MessageInterface"))
		assert_true(Interfaces.implements(message, &"CommandInterface"))
		var restored: RefCounted = message.get_script().new()
		restored.from_payload(message.to_payload())
		assert_eq(restored.to_payload(), message.to_payload())


func test_remaining_default_commands_are_invalid() -> void:
	assert_false(BuildStructureMessage.new().validate().is_empty())
	assert_false(CancelConstructionMessage.new().validate().is_empty())
	assert_false(RepairTargetMessage.new().validate().is_empty())
	assert_false(CaptureTargetMessage.new().validate().is_empty())
	assert_false(UpgradeStructureMessage.new().validate().is_empty())
	assert_false(CancelStructureUpgradeMessage.new().validate().is_empty())
	assert_false(TrainUnitsMessage.new().validate().is_empty())
	assert_false(CancelProductionMessage.new().validate().is_empty())
	assert_false(ReorderProductionMessage.new().validate().is_empty())
	assert_false(SetRallyPointMessage.new().validate().is_empty())
	assert_false(StartResearchMessage.new().validate().is_empty())
	assert_false(CancelResearchMessage.new().validate().is_empty())
	assert_false(SpecializeOutpostMessage.new().validate().is_empty())


func test_invalid_remaining_command_values_are_rejected() -> void:
	assert_false(
		BuildStructureMessage.new(
			"command-16",
			135,
			"worker-1",
			"building-1",
			Vector2.ZERO,
			0.0,
			999,
		).validate().is_empty()
	)
	assert_false(
		TrainUnitsMessage.new(
			"command-17",
			136,
			"factory-1",
			"unit-1",
			0,
		).validate().is_empty()
	)
	assert_false(
		ReorderProductionMessage.new(
			"command-18",
			137,
			"factory-1",
			"queue-item-1",
			-1,
		).validate().is_empty()
	)
	assert_false(
		SetRallyPointMessage.new(
			"command-19",
			138,
			PackedStringArray(["factory-1"]),
			{"kind": &"position"},
		).validate().is_empty()
	)
	assert_false(
		SpecializeOutpostMessage.new(
			"command-20",
			139,
			"outpost-1",
			999,
		).validate().is_empty()
	)
