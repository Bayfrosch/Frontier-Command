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
const STOP_UNITS: StringName = &"stop_units"
const HOLD_POSITION_UNITS: StringName = &"hold_position_units"
const PATROL_UNITS: StringName = &"patrol_units"
const SET_UNIT_STANCE: StringName = &"set_unit_stance"
const USE_ABILITY: StringName = &"use_ability"
const GATHER_RESOURCES: StringName = &"gather_resources"
const ATTACK_MOVE_UNITS: StringName = &"attack_move_units"
const ATTACK_TARGET: StringName = &"attack_target"
const BUILD_STRUCTURE: StringName = &"build_structure"
const CANCEL_CONSTRUCTION: StringName = &"cancel_construction"
const REPAIR_TARGET: StringName = &"repair_target"
const CAPTURE_TARGET: StringName = &"capture_target"
const UPGRADE_STRUCTURE: StringName = &"upgrade_structure"
const CANCEL_STRUCTURE_UPGRADE: StringName = &"cancel_structure_upgrade"
const TRAIN_UNITS: StringName = &"train_units"
const CANCEL_PRODUCTION: StringName = &"cancel_production"
const REORDER_PRODUCTION: StringName = &"reorder_production"
const SET_RALLY_POINT: StringName = &"set_rally_point"
const START_RESEARCH: StringName = &"start_research"
const CANCEL_RESEARCH: StringName = &"cancel_research"
const SPECIALIZE_OUTPOST: StringName = &"specialize_outpost"
const STATE_SNAPSHOT: StringName = &"state_snapshot"

enum MatchMode { ONE_VS_ONE, FREE_FOR_ALL, TEAM }
enum QueueMode { REPLACE, APPEND }
enum UnitStance { AGGRESSIVE, DEFENSIVE, HOLD_FIRE }
enum OutpostSpecialization { INDUSTRIAL, MILITARY, RESEARCH }
enum MatchPhase { LOBBY, RUNNING, PAUSED, ENDED }

#
