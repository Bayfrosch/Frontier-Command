using Godot;

[GlobalClass]
public partial class GameMessages : RefCounted
{
    public static readonly StringName DEBUG_SPAWN_UNIT = "debug_spawn_unit";
    public static readonly StringName CLIENT_HELLO = "client_hello";
    public static readonly StringName SERVER_HELLO = "server_hello";
    public static readonly StringName PING = "ping";
    public static readonly StringName PONG = "pong";
    public static readonly StringName DISCONNECT = "disconnect";
    public static readonly StringName MATCH_SETTINGS = "match_settings";
    public static readonly StringName CREATE_MATCH = "create_match";
    public static readonly StringName JOIN_MATCH = "join_match";
    public static readonly StringName LEAVE_MATCH = "leave_match";
    public static readonly StringName SET_PLAYER_READY = "set_player_ready";
    public static readonly StringName UPDATE_MATCH_SETTINGS = "update_match_settings";
    public static readonly StringName LOBBY_STATE = "lobby_state";
    public static readonly StringName START_MATCH = "start_match";
    public static readonly StringName MATCH_STARTED = "match_started";
    public static readonly StringName PAUSE_MATCH = "pause_match";
    public static readonly StringName MATCH_ENDED = "match_ended";
    public static readonly StringName MOVE_UNITS = "move_units";
    public static readonly StringName STOP_UNITS = "stop_units";
    public static readonly StringName HOLD_POSITION_UNITS = "hold_position_units";
    public static readonly StringName PATROL_UNITS = "patrol_units";
    public static readonly StringName SET_UNIT_STANCE = "set_unit_stance";
    public static readonly StringName USE_ABILITY = "use_ability";
    public static readonly StringName GATHER_RESOURCES = "gather_resources";
    public static readonly StringName ATTACK_MOVE_UNITS = "attack_move_units";
    public static readonly StringName ATTACK_TARGET = "attack_target";
    public static readonly StringName BUILD_STRUCTURE = "build_structure";
    public static readonly StringName CANCEL_CONSTRUCTION = "cancel_construction";
    public static readonly StringName REPAIR_TARGET = "repair_target";
    public static readonly StringName CAPTURE_TARGET = "capture_target";
    public static readonly StringName UPGRADE_STRUCTURE = "upgrade_structure";
    public static readonly StringName CANCEL_STRUCTURE_UPGRADE = "cancel_structure_upgrade";
    public static readonly StringName TRAIN_UNITS = "train_units";
    public static readonly StringName CANCEL_PRODUCTION = "cancel_production";
    public static readonly StringName SET_RALLY_POINT = "set_rally_point";
    public static readonly StringName START_RESEARCH = "start_research";
    public static readonly StringName CANCEL_RESEARCH = "cancel_research";
    public static readonly StringName CHOOSE_CAPITAL_UPGRADE = "choose_capital_upgrade";
    public static readonly StringName SPECIALIZE_OUTPOST = "specialize_outpost";
    public static readonly StringName COMMAND_RESULT = "command_result";
    public static readonly StringName STATE_SNAPSHOT = "state_snapshot";
    public static readonly StringName STATE_DELTA = "state_delta";
    public static readonly StringName RESYNC_REQUEST = "resync_request";
    public static readonly StringName RESYNC_RESPONSE = "resync_response";
    public static readonly StringName SIMULATION_EVENTS = "simulation_events";
    public static readonly StringName GENERIC_ERROR = "generic_error";
    public enum MatchMode {
        ONE_VS_ONE, FREE_FOR_ALL, TEAM
    }
    public enum QueueMode {
        REPLACE, APPEND
    }
    public enum UnitStance {
        AGGRESSIVE, DEFENSIVE, HOLD_FIRE
    }
    public enum OutpostSpecialization {
        INDUSTRIAL, MILITARY, RESEARCH
    }
    public enum MatchPhase {
        LOBBY, RUNNING, PAUSED, ENDED
    }
    public enum VisibilityState {
        HIDDEN, FOGGED, VISIBLE
    }
    public enum MovementCategory {
        GROUND, AIR, NAVAL
    }
}
