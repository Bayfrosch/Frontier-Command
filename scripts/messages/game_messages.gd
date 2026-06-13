class_name GameMessages

const MOVE_UNITS: StringName = &"move_units"
const ATTACK_TARGET: StringName = &"attack_target"
const STATE_SNAPSHOT: StringName = &"state_snapshot"

enum QueueMode { REPLACE, APPEND }
enum UnitStance { AGGRESSIVE, DEFENSIVE, HOLD_FIRE }
enum MatchPhase { LOBBY, RUNNING, PAUSED, ENDED }
