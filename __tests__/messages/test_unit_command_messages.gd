extends GutTest


func test_hold_position_round_trip() -> void:
	var message := HoldPositionMessage.new(
		"command-1",
		120,
		PackedStringArray(["unit-1"]),
		false,
	)
	var restored := HoldPositionMessage.new()
	restored.from_payload(message.to_payload())

	assert_eq(message.message_type, GameMessages.HOLD_POSITION_UNITS)
	assert_true(Interfaces.implements(message, &"MessageInterface"))
	assert_true(Interfaces.implements(message, &"CommandInterface"))
	assert_true(message.validate().is_empty())
	assert_eq(restored.to_payload(), message.to_payload())


func test_patrol_units_round_trip() -> void:
	var message := PatrolUnitsMessage.new(
		"command-2",
		121,
		PackedStringArray(["unit-1"]),
		Vector2(10.0, 20.0),
		GameMessages.QueueMode.APPEND,
	)
	var restored := PatrolUnitsMessage.new()
	restored.from_payload(message.to_payload())

	assert_true(Interfaces.implements(message, &"QueueableCommandInterface"))
	assert_true(message.validate().is_empty())
	assert_eq(restored.to_payload(), message.to_payload())


func test_set_unit_stance_round_trip() -> void:
	var message := SetUnitStanceMessage.new(
		"command-3",
		122,
		PackedStringArray(["unit-1"]),
		GameMessages.UnitStance.HOLD_FIRE,
	)
	var restored := SetUnitStanceMessage.new()
	restored.from_payload(message.to_payload())

	assert_true(message.validate().is_empty())
	assert_eq(restored.to_payload(), message.to_payload())


func test_use_ability_supports_each_optional_target_form() -> void:
	var entity_target := UseAbilityMessage.new(
		"command-4",
		123,
		PackedStringArray(["unit-1"]),
		"ability-1",
		GameMessages.QueueMode.REPLACE,
		"enemy-1",
	)
	var position_target := UseAbilityMessage.new(
		"command-5",
		124,
		PackedStringArray(["unit-1"]),
		"ability-2",
		GameMessages.QueueMode.APPEND,
		"",
		Vector2.ZERO,
	)

	assert_true(entity_target.validate().is_empty())
	assert_true(position_target.validate().is_empty())
	assert_false(entity_target.to_payload().has("target_position"))
	assert_false(position_target.to_payload().has("target_entity_id"))


func test_use_ability_rejects_multiple_targets() -> void:
	var message := UseAbilityMessage.new(
		"command-6",
		125,
		PackedStringArray(["unit-1"]),
		"ability-1",
		GameMessages.QueueMode.REPLACE,
		"enemy-1",
		Vector2(10.0, 20.0),
	)

	assert_eq(message.validate().size(), 1)


func test_gather_resources_omits_optional_refinery() -> void:
	var message := GatherResourcesMessage.new(
		"command-7",
		126,
		PackedStringArray(["harvester-1"]),
		"resource-field-1",
	)
	var restored := GatherResourcesMessage.new()
	restored.from_payload(message.to_payload())

	assert_true(message.validate().is_empty())
	assert_false(message.to_payload().has("refinery_entity_id"))
	assert_eq(restored.to_payload(), message.to_payload())


func test_default_unit_commands_are_invalid() -> void:
	assert_false(HoldPositionMessage.new().validate().is_empty())
	assert_false(PatrolUnitsMessage.new().validate().is_empty())
	assert_false(SetUnitStanceMessage.new().validate().is_empty())
	assert_false(UseAbilityMessage.new().validate().is_empty())
	assert_false(GatherResourcesMessage.new().validate().is_empty())
