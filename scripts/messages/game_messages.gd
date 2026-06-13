class_name GameMessages

const CLIENT_HELLO: StringName = &"client_hello"
const SERVER_HELLO: StringName = &"server_hello"
const PING: StringName = &"ping"
const PONG: StringName = &"pong"
const DISCONNECT: StringName = &"disconnect"

const MOVE_UNITS: StringName = &"move_units"
const ATTACK_TARGET: StringName = &"attack_target"
const STATE_SNAPSHOT: StringName = &"state_snapshot"

enum QueueMode { REPLACE, APPEND }
enum UnitStance { AGGRESSIVE, DEFENSIVE, HOLD_FIRE }
enum MatchPhase { LOBBY, RUNNING, PAUSED, ENDED }

#