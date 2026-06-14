extends GutTest

const VALID_SETTINGS := {
	"match_name": "Frontier Match",
	"max_players": 2,
	"map_seed": "seed",
	"map_template": "standard_1v1",
	"map_width_tiles": 192,
	"map_height_tiles": 192,
	"allow_pause": true,
}


func test_match_settings_round_trip() -> void:
	var message := MatchSettingsMessage.new(
		"Frontier Match",
		2,
		"seed",
		"standard_1v1",
		192,
		192,
		true,
	)
	assert_true(message.validate().is_empty())

	var restored := MatchSettingsMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.MATCH_SETTINGS)
	assert_eq(restored.to_payload(), message.to_payload())


func test_create_match_round_trip() -> void:
	var message := CreateMatchMessage.new(VALID_SETTINGS)
	assert_true(message.validate().is_empty())

	var restored := CreateMatchMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.CREATE_MATCH)
	assert_eq(restored.settings, message.settings)


func test_join_match_round_trip() -> void:
	var message := JoinMatchMessage.new("match-1", true)
	assert_true(message.validate().is_empty())

	var restored := JoinMatchMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.JOIN_MATCH)
	assert_eq(restored.match_id, message.match_id)
	assert_eq(restored.as_observer, message.as_observer)


func test_leave_match_has_empty_payload() -> void:
	var message := LeaveMatchMessage.new()
	assert_eq(message.message_type, GameMessages.LEAVE_MATCH)
	assert_eq(message.to_payload(), {})
	assert_true(message.validate().is_empty())


func test_set_player_ready_round_trip() -> void:
	var message := SetPlayerReadyMessage.new(
		true,
		"frontier_coalition",
		"team-1",
	)
	assert_true(message.validate().is_empty())

	var restored := SetPlayerReadyMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.SET_PLAYER_READY)
	assert_eq(restored.is_ready, message.is_ready)
	assert_eq(restored.faction_id, message.faction_id)
	assert_eq(restored.team_id, message.team_id)


func test_set_player_ready_omits_empty_team() -> void:
	var message := SetPlayerReadyMessage.new(true, "frontier_coalition")
	assert_false(message.to_payload().has("team_id"))


func test_update_match_settings_round_trip() -> void:
	var message := UpdateMatchSettingsMessage.new(VALID_SETTINGS)
	assert_true(message.validate().is_empty())

	var restored := UpdateMatchSettingsMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.UPDATE_MATCH_SETTINGS)
	assert_eq(restored.settings, message.settings)


func test_lobby_state_round_trip() -> void:
	var players: Array[Dictionary] = [{
		"player_id": "player-1",
		"display_name": "Player",
		"is_ai": false,
		"is_ready": true,
		"team_id": 1,
		"faction": "frontier_coalition",
		"color": Color.RED,
	}]
	var message := LobbyStateMessage.new(
		"match-1",
		"player-1",
		VALID_SETTINGS,
		players,
	)
	assert_true(message.validate().is_empty())

	var restored := LobbyStateMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.LOBBY_STATE)
	assert_eq(restored.lobby_id, message.lobby_id)
	assert_eq(restored.host_player_id, message.host_player_id)
	assert_eq(restored.settings, message.settings)
	assert_eq(restored.players, message.players)


func test_start_match_has_empty_payload() -> void:
	var message := StartMatchMessage.new()
	assert_eq(message.message_type, GameMessages.START_MATCH)
	assert_eq(message.to_payload(), {})
	assert_true(message.validate().is_empty())


func test_match_started_round_trip() -> void:
	var message := MatchStartedMessage.new(
		"match-1",
		0,
		20,
		"seed",
		"player-1",
	)
	assert_true(message.validate().is_empty())

	var restored := MatchStartedMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.MATCH_STARTED)
	assert_eq(restored.match_id, message.match_id)
	assert_eq(restored.start_tick, message.start_tick)
	assert_eq(restored.tick_rate, message.tick_rate)
	assert_eq(restored.map_seed, message.map_seed)
	assert_eq(restored.local_player_id, message.local_player_id)


func test_match_started_omits_observer_player_id() -> void:
	var message := MatchStartedMessage.new("match-1", 0, 20, "seed")
	assert_false(message.to_payload().has("local_player_id"))


func test_pause_match_has_empty_payload() -> void:
	var message := PauseMatchMessage.new()
	assert_eq(message.message_type, GameMessages.PAUSE_MATCH)
	assert_eq(message.to_payload(), {})
	assert_true(message.validate().is_empty())


func test_match_ended_round_trip() -> void:
	var message := MatchEndedMessage.new(
		10000,
		PackedStringArray(["player-1"]),
		PackedStringArray(["player-2"]),
		&"victory_conditions",
		"team-1",
	)
	assert_true(message.validate().is_empty())

	var restored := MatchEndedMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.MATCH_ENDED)
	assert_eq(restored.end_tick, message.end_tick)
	assert_eq(restored.winner_player_ids, message.winner_player_ids)
	assert_eq(restored.defeated_player_ids, message.defeated_player_ids)
	assert_eq(restored.reason, message.reason)
	assert_eq(restored.winner_team_id, message.winner_team_id)


func test_invalid_lobby_messages_are_rejected() -> void:
	assert_false(MatchSettingsMessage.new().validate().is_empty())
	assert_false(CreateMatchMessage.new().validate().is_empty())
	assert_false(JoinMatchMessage.new().validate().is_empty())
	assert_false(SetPlayerReadyMessage.new().validate().is_empty())
	assert_false(UpdateMatchSettingsMessage.new().validate().is_empty())
	assert_false(LobbyStateMessage.new().validate().is_empty())
	assert_false(MatchStartedMessage.new().validate().is_empty())
	assert_false(MatchEndedMessage.new().validate().is_empty())
