class_name GameMessages

const CLIENT_HELLO: StringName = &"client_hello"
const SERVER_HELLO: StringName = &"server_hello"
const PING: StringName = &"ping"
const PONG: StringName = &"pong"
const DISCONNECT: StringName = &"disconnect"

const MATCH_SETTINGS: StringName = &"match_settings"
const CREATE_MATCH: StringName = &"create_match"
const JOIN_MATCH: StringName = &"join_match"
const LEAVE_MATCH: StringName = &"leave_match"
const SET_PLAYER_READY: StringName = &"set_player_ready"
const UPDATE_MATCH_SETTINGS: StringName = &"update_match_settings"
const LOBBY_STATE: StringName = &"lobby_state"
const START_MATCH: StringName = &"start_match"
const MATCH_STARTED: StringName = &"match_started"
const PAUSE_MATCH: StringName = &"pause_match"
const MATCH_ENDED: StringName = &"match_ended"

const MOVE_UNITS: StringName = &"move_units"
const ATTACK_TARGET: StringName = &"attack_target"
const STATE_SNAPSHOT: StringName = &"state_snapshot"

enum MatchMode { ONE_VS_ONE, FREE_FOR_ALL, TEAM }
enum QueueMode { REPLACE, APPEND }
enum UnitStance { AGGRESSIVE, DEFENSIVE, HOLD_FIRE }
enum MatchPhase { LOBBY, RUNNING, PAUSED, ENDED }
