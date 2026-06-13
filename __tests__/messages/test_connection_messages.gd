extends GutTest


func test_client_hello_round_trip() -> void:
	var message := ClientHelloMessage.new("0.1.0", "Player")
	assert_true(message.validate().is_empty())
	assert_false(message.to_payload().has("reconnect_token"))

	var restored := ClientHelloMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.CLIENT_HELLO)
	assert_eq(restored.client_version, message.client_version)
	assert_eq(restored.player_name, message.player_name)


func test_server_hello_round_trip() -> void:
	var message := ServerHelloMessage.new(
		"0.1.0",
		1,
		"player-1",
		"reconnect-token",
		1000,
	)
	assert_true(message.validate().is_empty())

	var restored := ServerHelloMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.SERVER_HELLO)
	assert_eq(restored.server_version, message.server_version)
	assert_eq(restored.protocol_version, message.protocol_version)
	assert_eq(restored.player_id, message.player_id)
	assert_eq(restored.reconnect_token, message.reconnect_token)
	assert_eq(restored.server_time_unix_ms, message.server_time_unix_ms)


func test_ping_round_trip() -> void:
	var message := PingMessage.new(1000)
	assert_true(message.validate().is_empty())

	var restored := PingMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.PING)
	assert_eq(restored.client_time_unix_ms, message.client_time_unix_ms)


func test_pong_round_trip() -> void:
	var message := PongMessage.new(1000, 1010)
	assert_true(message.validate().is_empty())

	var restored := PongMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.PONG)
	assert_eq(restored.client_time_unix_ms, message.client_time_unix_ms)
	assert_eq(restored.server_time_unix_ms, message.server_time_unix_ms)


func test_disconnect_round_trip_without_details() -> void:
	var message := DisconnectMessage.new(&"client_quit")
	assert_true(message.validate().is_empty())
	assert_false(message.to_payload().has("details"))

	var restored := DisconnectMessage.new()
	restored.from_payload(message.to_payload())
	assert_eq(restored.message_type, GameMessages.DISCONNECT)
	assert_eq(restored.reason, message.reason)


func test_disconnect_rejects_unknown_reason() -> void:
	var message := DisconnectMessage.new(&"unknown")
	assert_false(message.validate().is_empty())

#