# Frontier Command Message Protocol

## Purpose

This document defines the GDScript messages exchanged between the Client,
Command Handler, Simulation Core, AI, and multiplayer server.

Commands describe intent. The authoritative Simulation Core validates commands,
changes game state, and emits results. Clients and AI never modify authoritative
state directly.

Frontier Command uses the
[`gdscript-interfaces`](https://github.com/moritz-junge/gdscript-interfaces)
Godot 4 port to define runtime-checked interfaces. Concrete messages are typed
`RefCounted` classes. They are converted to `Dictionary` values only when they
cross the network boundary.

The library validates the existence of properties, methods, and signals. It
cannot validate property types, method parameter types, or return types. Static
GDScript annotations and explicit message validation remain required.

## GDScript Conventions

- The project targets Godot 4.6.
- Message classes implement `MessageInterface`.
- Gameplay command classes also implement `CommandInterface`.
- Queueable command classes also implement `QueueableCommandInterface`.
- Message and event names use `StringName`.
- IDs use `String`.
- Positions use Godot's `Vector2`.
- Lists of entity IDs use `PackedStringArray`.
- Structured lists use `Array[Dictionary]`.
- Optional keys may be omitted. Use `null` only to explicitly clear a value.
- Time values are milliseconds unless stated otherwise.
- Simulation time uses an integer `tick`.
- Message keys use `snake_case`, following GDScript conventions.
- `to_payload()` must return only Variant-compatible values.
- `from_payload()` is called only after the Message Factory validates raw types.
- The client never sends calculated costs, damage, progress, or outcomes.
- State and events sent to a client must be filtered by fog of war.

## Interface Library

Install the Godot 4 port under `res://addons/gdscript-interfaces/`, enable the
plugin, and keep its `Interfaces` autoload enabled.

Implementing classes use script preloads because GDScript global `class_name`
references cannot be used directly in a constant:

```gdscript
const implements: Array[GDScript] = [
	preload("res://scripts/messages/interfaces/message_interface.gd"),
]
```

Check an object before accepting it:

```gdscript
if not Interfaces.implements(message, MessageInterface):
	push_error("Object does not implement MessageInterface.")
	return
```

Keep `allow_string_classes` disabled. Preloads are explicit and avoid the
library's string-evaluation workaround.

Recommended plugin configuration:

```gdscript
@export var runtime_validation: bool = false
@export var allow_string_classes: bool = false
@export var strict_validation: bool = true
@export var validate_dirs: Array[String] = ["res://scripts/messages/"]
```

## Message Interfaces

### MessageInterface

Every message implements:

```gdscript
# Interface
class_name MessageInterface
extends RefCounted

var message_type: StringName

func to_payload() -> Dictionary:
	return {}

func from_payload(_payload: Dictionary) -> void:
	pass

func validate() -> PackedStringArray:
	return PackedStringArray()
```

`validate()` returns an empty array when valid. Each entry otherwise describes
one validation error.

### CommandInterface

Every gameplay command implements both `MessageInterface` and
`CommandInterface`:

```gdscript
# Interface
class_name CommandInterface
extends RefCounted

var command_id: String
var issued_at_tick: int
```

### QueueableCommandInterface

Commands that can replace or append to an order queue also implement:

```gdscript
# Interface
class_name QueueableCommandInterface
extends RefCounted

var queue_mode: int
```

### Concrete Message Example

Each message is a typed class. This is the implementation shape for
`MOVE_UNITS`:

```gdscript
class_name MoveUnitsMessage
extends RefCounted

const implements: Array[GDScript] = [
	preload("res://scripts/messages/interfaces/message_interface.gd"),
	preload("res://scripts/messages/interfaces/command_interface.gd"),
	preload(
		"res://scripts/messages/interfaces/queueable_command_interface.gd"
	),
]

var message_type: StringName = GameMessages.MOVE_UNITS
var command_id: String
var issued_at_tick: int
var queue_mode: int = GameMessages.QueueMode.REPLACE
var unit_ids: PackedStringArray
var destination: Vector2
var formation: StringName = &"rectangle"

func to_payload() -> Dictionary:
	return {
		"command_id": command_id,
		"issued_at_tick": issued_at_tick,
		"queue_mode": queue_mode,
		"unit_ids": unit_ids,
		"destination": destination,
		"formation": formation,
	}

func from_payload(payload: Dictionary) -> void:
	command_id = payload.get("command_id", "")
	issued_at_tick = payload.get("issued_at_tick", -1)
	queue_mode = payload.get(
		"queue_mode",
		GameMessages.QueueMode.REPLACE
	)
	unit_ids = payload.get("unit_ids", PackedStringArray())
	destination = payload.get("destination", Vector2.ZERO)
	formation = payload.get("formation", &"rectangle")

func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if command_id.is_empty():
		errors.append("command_id is required.")
	if issued_at_tick < 0:
		errors.append("issued_at_tick cannot be negative.")
	if queue_mode not in [
		GameMessages.QueueMode.REPLACE,
		GameMessages.QueueMode.APPEND,
	]:
		errors.append("queue_mode is invalid.")
	if unit_ids.is_empty():
		errors.append("unit_ids must contain at least one unit.")
	if formation not in [&"none", &"rectangle"]:
		errors.append("formation is invalid.")
	return errors
```

The sections below document each concrete class's fields and serialized payload.
Use one `.gd` file per concrete message.

## Common ID Types

GDScript cannot create aliases such as `PlayerId` or `EntityId`. These values are
all represented by `String`, but should be named clearly:

```gdscript
var message_id: String
var request_id: String
var match_id: String
var player_id: String
var team_id: String
var entity_id: String
var command_id: String
var definition_id: String
var research_id: String
var ability_id: String
```

IDs are opaque. Code must compare them, not extract information from them.

## Message Envelope

Every multiplayer message uses this outer structure:

```gdscript
{
	"protocol_version": 1,             # int
	"message_id": "unique-message-id", # String
	"message_type": &"move_units",     # StringName
	"sent_at_unix_ms": 0,              # int
	"payload": {},                      # Dictionary

	# Optional:
	"match_id": "match-id",             # String
	"sender_player_id": "player-id",    # String
	"request_id": "request-id",         # String
}
```

Suggested constructor:

```gdscript
static func create_envelope(
		message_type: StringName,
		payload: Dictionary,
		message_id: String,
		sent_at_unix_ms: int,
		match_id: String = "",
		request_id: String = "") -> Dictionary:
	var envelope: Dictionary = {
		"protocol_version": PROTOCOL_VERSION,
		"message_id": message_id,
		"message_type": message_type,
		"sent_at_unix_ms": sent_at_unix_ms,
		"payload": payload,
	}
	if not match_id.is_empty():
		envelope["match_id"] = match_id
	if not request_id.is_empty():
		envelope["request_id"] = request_id
	return envelope
```

The server derives `sender_player_id` from the authenticated peer. It must not
trust a client-provided player ID.

## Command Base

Every gameplay command payload contains:

```gdscript
{
	"command_id": "unique-command-id", # String
	"issued_at_tick": 120,             # int
}
```

Queueable commands also contain:

```gdscript
{
	"queue_mode": GameMessages.QueueMode.REPLACE, # QueueMode
}
```

`player_id` is not part of a client command payload. The server associates the
command with its authenticated network peer. Local AI code passes its player ID
to the Command Handler separately.

## Connection Messages

### CLIENT_HELLO

```gdscript
{
	"client_version": "0.1.0", # String
	"protocol_version": 1,     # int
	"player_name": "Player",   # String

	# Optional:
	"reconnect_token": "token", # String
}
```

### SERVER_HELLO

```gdscript
{
	"server_version": "0.1.0", # String
	"protocol_version": 1,     # int
	"player_id": "player-id",  # String
	"reconnect_token": "token",# String
	"server_time_unix_ms": 0,  # int
}
```

### PING

```gdscript
{
	"client_time_unix_ms": 0, # int
}
```

### PONG

```gdscript
{
	"client_time_unix_ms": 0, # int
	"server_time_unix_ms": 0, # int
}
```

### DISCONNECT

```gdscript
{
	"reason": &"client_quit", # StringName:
	# client_quit, timeout, kicked, server_shutdown, protocol_error

	# Optional:
	"details": "", # String
}
```

## Lobby And Match Messages

### Match Settings

```gdscript
{
	"match_name": "Frontier Match",       # String
	"mode": GameMessages.MatchMode.ONE_VS_ONE,
	"max_players": 2,                     # int
	"map_template_id": "standard_1v1",    # String
	"map_seed": "seed",                   # String
	"map_width_tiles": 192,               # int
	"map_height_tiles": 192,              # int
	"allow_observers": true,              # bool
	"allow_pause": true,                  # bool
}
```

### CREATE_MATCH

```gdscript
{
	"settings": {}, # Match Settings Dictionary
}
```

### JOIN_MATCH

```gdscript
{
	"match_id": "match-id", # String
	"as_observer": false,   # bool
}
```

### LEAVE_MATCH

```gdscript
{}
```

### SET_PLAYER_READY

```gdscript
{
	"is_ready": true,                  # bool
	"faction_id": "frontier_coalition",# String

	# Optional in free-for-all:
	"team_id": "team-1",               # String
}
```

### UPDATE_MATCH_SETTINGS

Host-only command.

```gdscript
{
	"settings": {}, # Match Settings Dictionary
}
```

### LOBBY_STATE

```gdscript
{
	"match_id": "match-id",         # String
	"host_player_id": "player-id",  # String
	"settings": {},                 # Match Settings Dictionary
	"players": [                    # Array[Dictionary]
		{
			"player_id": "player-id",
			"display_name": "Player",
			"is_ai": false,
			"is_ready": true,
			"is_connected": true,

			# Optional:
			"team_id": "team-1",
			"faction_id": "frontier_coalition",
			"ai_difficulty": &"normal",
		},
	],
}
```

### START_MATCH

```gdscript
{}
```

### MATCH_STARTED

```gdscript
{
	"match_id": "match-id", # String
	"start_tick": 0,        # int
	"tick_rate": 20,        # int
	"map_seed": "seed",     # String

	# Optional for observers:
	"local_player_id": "player-id", # String
}
```

### PAUSE_MATCH / RESUME_MATCH / SURRENDER

Each uses an empty payload:

```gdscript
{}
```

### MATCH_ENDED

```gdscript
{
	"end_tick": 10000,                       # int
	"winner_player_ids": PackedStringArray(),# PackedStringArray
	"defeated_player_ids": PackedStringArray(),
	"reason": &"victory_conditions",         # StringName

	# Optional:
	"winner_team_id": "team-1",              # String
}
```

Valid reasons are `victory_conditions`, `surrender`,
`all_opponents_disconnected`, and `admin_ended`.

## Unit Commands

Every example below also includes the Command Base keys.

### MOVE_UNITS

The authoritative simulation already owns each unit's current position, so the
client does not send a `from` position.

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"unit_ids": PackedStringArray(["unit-1", "unit-2"]),
	"destination": Vector2(100.0, 250.0),
	"formation": &"rectangle", # none or rectangle
}
```

### ATTACK_MOVE_UNITS

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"unit_ids": PackedStringArray(["unit-1", "unit-2"]),
	"destination": Vector2(100.0, 250.0),
	"formation": &"rectangle",
}
```

### ATTACK_TARGET

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"unit_ids": PackedStringArray(["unit-1", "unit-2"]),
	"target_entity_id": "enemy-1",
}
```

### STOP_UNITS

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"unit_ids": PackedStringArray(["unit-1", "unit-2"]),
	"clear_queue": true,
}
```

### HOLD_POSITION

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"unit_ids": PackedStringArray(["unit-1"]),
	"enabled": true,
}
```

### PATROL_UNITS

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"unit_ids": PackedStringArray(["unit-1"]),
	"destination": Vector2(100.0, 250.0),
}
```

### SET_UNIT_STANCE

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"unit_ids": PackedStringArray(["unit-1"]),
	"stance": GameMessages.UnitStance.DEFENSIVE,
}
```

### USE_ABILITY

Supply at most one target form. Omit both for an ability without a target.

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"caster_entity_ids": PackedStringArray(["unit-1"]),
	"ability_id": "ability-id",

	# Optional, choose one:
	"target_entity_id": "enemy-1",
	# "target_position": Vector2(100.0, 250.0),
}
```

### GATHER_RESOURCES

Harvesters repeat the gather-deliver cycle until stopped, the field is depleted,
or no valid refinery remains.

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"harvester_entity_ids": PackedStringArray(["harvester-1"]),
	"resource_field_entity_id": "resource-field-1",

	# Optional; otherwise the simulation selects a valid owned refinery:
	"refinery_entity_id": "refinery-1",
}
```

## Construction And Support Commands

### BUILD_STRUCTURE

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"construction_unit_id": "worker-1",
	"building_definition_id": "basic_generator",
	"position": Vector2(100.0, 250.0),
	"rotation_radians": 0.0,
}
```

### CANCEL_CONSTRUCTION

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"construction_site_id": "construction-site-1",
}
```

### REPAIR_TARGET

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"repair_unit_ids": PackedStringArray(["worker-1"]),
	"target_entity_id": "building-1",
}
```

### CAPTURE_TARGET

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"queue_mode": GameMessages.QueueMode.REPLACE,
	"envoy_unit_ids": PackedStringArray(["envoy-1"]),
	"target_entity_id": "outpost-1",
}
```

### UPGRADE_STRUCTURE

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"structure_entity_id": "building-1",
	"upgrade_definition_id": "advanced_production",
}
```

### CANCEL_STRUCTURE_UPGRADE

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"structure_entity_id": "building-1",
	"upgrade_queue_item_id": "queue-item-1",
}
```

## Production Commands

### TRAIN_UNITS

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"producer_entity_id": "factory-1",
	"unit_definition_id": "main_battle_unit",
	"quantity": 3,
}
```

### CANCEL_PRODUCTION

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"producer_entity_id": "factory-1",
	"queue_item_id": "queue-item-1",
}
```

### REORDER_PRODUCTION

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"producer_entity_id": "factory-1",
	"queue_item_id": "queue-item-1",
	"new_index": 0,
}
```

### SET_RALLY_POINT

Position target:

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"producer_entity_ids": PackedStringArray(["factory-1"]),
	"target": {
		"kind": &"position",
		"position": Vector2(100.0, 250.0),
	},
}
```

Entity target:

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"producer_entity_ids": PackedStringArray(["factory-1"]),
	"target": {
		"kind": &"entity",
		"entity_id": "entity-1",
	},
}
```

Use `"target": null` to clear the rally point.

## Research And Progression Commands

### START_RESEARCH

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"research_structure_id": "research-center-1",
	"research_id": "research-id",
}
```

### CANCEL_RESEARCH

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"research_structure_id": "research-center-1",
	"research_queue_item_id": "queue-item-1",
}
```

### CHOOSE_DOCTRINE

Doctrine selection is permanent for the match.

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"doctrine_id": "mechanized_command",
}
```

### CHOOSE_CAPITAL_UPGRADE

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"capital_unit_entity_id": "capital-unit-1",
	"upgrade_definition_id": "capital-upgrade-id",
}
```

### SPECIALIZE_OUTPOST

```gdscript
{
	"command_id": "command-id",
	"issued_at_tick": 120,
	"outpost_entity_id": "outpost-1",
	"specialization": GameMessages.OutpostSpecialization.INDUSTRIAL,
}
```

## Command Result

Every command receives one initial result. Acceptance means the command was
valid when submitted; it does not guarantee that its objective will complete.

Accepted:

```gdscript
{
	"command_id": "command-id",
	"status": &"accepted",
	"accepted_at_tick": 121,
}
```

Rejected:

```gdscript
{
	"command_id": "command-id",
	"status": &"rejected",
	"rejection": {
		"code": &"not_owner",
		"details": "The player does not own every selected unit.",
		"entity_ids": PackedStringArray(["unit-1"]),
	},
}
```

Rejection codes:

```gdscript
const REJECTION_CODES: Array[StringName] = [
	&"malformed_command",
	&"player_not_in_match",
	&"player_defeated",
	&"not_owner",
	&"entity_not_found",
	&"entity_destroyed",
	&"invalid_target",
	&"target_not_visible",
	&"unsupported_command",
	&"invalid_placement",
	&"unreachable_destination",
	&"insufficient_materials",
	&"insufficient_energy",
	&"fleet_capacity_exceeded",
	&"missing_prerequisite",
	&"research_locked",
	&"already_researched",
	&"doctrine_already_chosen",
	&"queue_full",
	&"match_not_running",
	&"rate_limited",
]
```

## Authoritative State

### Player State

```gdscript
{
	"player_id": "player-id",              # String
	"faction_id": "frontier_coalition",    # String
	"materials": 15000,                    # int
	"energy_produced": 20,                 # int
	"energy_required": 10,                 # int
	"is_low_power": false,                 # bool
	"fleet_capacity_used": 5,              # int
	"fleet_capacity_reserved": 2,          # int
	"fleet_capacity_limit": 20,            # int
	"completed_research_ids": PackedStringArray(),
	"is_defeated": false,                  # bool
	"is_connected": true,                  # bool

	# Optional:
	"team_id": "team-1",                   # String
	"doctrine_id": "mechanized_command",   # String
}
```

### Entity State

```gdscript
{
	"entity_id": "unit-1",             # String
	"definition_id": "scout_unit",     # String
	"kind": &"unit",                   # StringName
	"position": Vector2.ZERO,          # Vector2
	"rotation_radians": 0.0,           # float
	"health": 100,                     # int
	"max_health": 100,                 # int
	"is_active": true,                 # bool
	"visibility": GameMessages.VisibilityState.VISIBLE,

	# Optional:
	"owner_player_id": "player-id",    # String
	"unit": {},                        # Unit State Dictionary
	"building": {},                    # Building State Dictionary
	"outpost": {},                     # Outpost State Dictionary
	"resource_field": {},              # Resource Field State Dictionary
}
```

Only the subtype matching `kind` should be present.

### Unit State

```gdscript
{
	"movement_category": GameMessages.MovementCategory.GROUND,
	"veterancy_level": 1,              # int
	"experience": 0,                   # int
	"stance": GameMessages.UnitStance.DEFENSIVE,
	"is_moving": false,                # bool
	"status_effect_ids": PackedStringArray(),

	# Optional:
	"current_command_id": "command-id",# String
}
```

### Building State

```gdscript
{
	"construction_progress": 1.0, # float from 0.0 to 1.0
	"is_powered": true,           # bool
	"production_queue": [],       # Array[Dictionary]
	"research_queue": [],         # Array[Dictionary]
	"upgrade_queue": [],          # Array[Dictionary]

	# Optional:
	"rally_target": {},           # Rally Target Dictionary
}
```

Each queue item uses:

```gdscript
{
	"queue_item_id": "queue-item-1", # String
	"definition_id": "definition-id",# String
	"progress": 0.5,                 # float from 0.0 to 1.0
	"state": &"active",              # waiting, active, or paused
}
```

For research use `research_id`; for structure upgrades use
`upgrade_definition_id` instead of `definition_id`.

### Outpost State

```gdscript
{
	"capture_progress": 0.0, # float from 0.0 to 1.0

	# Optional:
	"specialization": GameMessages.OutpostSpecialization.INDUSTRIAL,
	"capturing_player_id": "player-id",
}
```

### Resource Field State

```gdscript
{
	"materials_remaining": 200000, # int
	"is_depleted": false,          # bool
}
```

## State Synchronization

### STATE_SNAPSHOT

A complete fog-filtered state sent at match start, observer join, reconnect, or
resynchronization.

```gdscript
{
	"match_id": "match-id",       # String
	"tick": 120,                  # int
	"phase": GameMessages.MatchPhase.RUNNING,
	"match_time_ms": 6000,        # int
	"players": [],                # Array[Dictionary]
	"entities": [],               # Array[Dictionary]
	"explored_map_data": PackedByteArray(),
	"checksum": "checksum",       # String
}
```

### STATE_DELTA

```gdscript
{
	"from_tick": 120,             # int
	"to_tick": 121,               # int
	"changed_players": [],        # Array[Dictionary]
	"upserted_entities": [],      # Array[Dictionary]
	"removed_entity_ids": PackedStringArray(),
	"checksum": "checksum",       # String

	# Optional:
	"explored_map_patch": PackedByteArray(),
}
```

`removed_entity_ids` includes destroyed entities and enemy entities that left
visibility. The client must not infer which occurred unless it also receives an
event it is allowed to know.

### RESYNC_REQUEST

```gdscript
{
	"client_tick": 120,             # int
	"client_checksum": "checksum",  # String
	"reason": &"checksum_mismatch", # missing_delta, checksum_mismatch, reconnect
}
```

### RESYNC_RESPONSE

```gdscript
{
	"snapshot": {}, # State Snapshot Dictionary
}
```

## Simulation Events

Important outcomes are sent in batches:

```gdscript
{
	"tick": 121,
	"events": [], # Array[Dictionary]
}
```

Every event contains:

```gdscript
{
	"event_id": "event-id",       # String
	"event_type": &"unit_created",# StringName
	"tick": 121,                  # int
}
```

Event-specific fields:

| `event_type` | Required fields |
| --- | --- |
| `command_started` | `command_id: String`, `entity_ids: PackedStringArray` |
| `command_completed` | `command_id: String`, `entity_ids: PackedStringArray` |
| `command_failed` | `command_id: String`, `entity_ids: PackedStringArray`, `reason: StringName` |
| `entity_created` | `entity: Dictionary` |
| `entity_destroyed` | `entity_id: String`; optional `owner_player_id`, `destroyed_by_entity_id` |
| `construction_completed` | `building_entity_id: String`, `builder_entity_id: String` |
| `production_completed` | `producer_entity_id: String`, `produced_entity_id: String`, `queue_item_id: String` |
| `research_completed` | `player_id: String`, `research_id: String` |
| `doctrine_chosen` | `player_id: String`, `doctrine_id: String` |
| `ability_activated` | `ability_id: String`, `caster_entity_ids: PackedStringArray`; optional target |
| `projectile_created` | `projectile_entity_id`, `source_entity_id`, `target_position`; optional `target_entity_id` |
| `damage_applied` | `target_entity_id: String`, `amount: int`, `remaining_health: int`; optional source |
| `veterancy_gained` | `unit_entity_id: String`, `new_level: int` |
| `capital_level_gained` | `unit_entity_id: String`, `new_level: int`, `available_upgrade_ids: PackedStringArray` |
| `outpost_captured` | `outpost_entity_id: String`, `new_owner_player_id: String`; optional previous owner |
| `outpost_specialized` | `outpost_entity_id: String`, `specialization: OutpostSpecialization` |
| `resource_delivered` | `harvester_entity_id`, `refinery_entity_id`, `player_id`, `materials: int` |
| `resource_field_depleted` | `resource_field_entity_id: String` |
| `power_state_changed` | `player_id: String`, `is_low_power: bool`, `energy_produced: int`, `energy_required: int` |
| `player_under_attack` | `player_id: String`, `approximate_position: Vector2` |
| `player_defeated` | `player_id: String` |
| `destination_unreachable` | `command_id`, `unit_entity_ids`, `requested_destination`; optional nearest position |

Example:

```gdscript
{
	"event_id": "event-id",
	"event_type": &"destination_unreachable",
	"tick": 121,
	"command_id": "command-id",
	"unit_entity_ids": PackedStringArray(["unit-1"]),
	"requested_destination": Vector2(100.0, 250.0),
	"nearest_reachable_position": Vector2(95.0, 245.0),
}
```

## Generic Error

Used for protocol, authorization, lobby, and server failures that are not
gameplay command rejections.

```gdscript
{
	"code": &"match_not_found", # StringName
	"retryable": false,         # bool

	# Optional:
	"details": "",              # String
}
```

Valid codes:

```gdscript
const ERROR_CODES: Array[StringName] = [
	&"unsupported_protocol_version",
	&"authentication_failed",
	&"permission_denied",
	&"match_not_found",
	&"match_full",
	&"invalid_match_settings",
	&"player_not_ready",
	&"request_timeout",
	&"internal_server_error",
]
```

## Validation

The interface library validates implementation shape, while a Message Factory
validates untrusted wire data. Both checks are required.

```gdscript
static func create_move_units(payload: Dictionary) -> MoveUnitsMessage:
	if not payload.has_all([
		"command_id",
		"issued_at_tick",
		"queue_mode",
		"unit_ids",
		"destination",
	]):
		return null

	if not payload["command_id"] is String:
		return null
	if not payload["issued_at_tick"] is int:
		return null
	if not payload["queue_mode"] is int:
		return null
	if not payload["unit_ids"] is PackedStringArray:
		return null
	if not payload["destination"] is Vector2:
		return null

	var message := MoveUnitsMessage.new()
	if not Interfaces.implements(
			message,
			[MessageInterface, CommandInterface, QueueableCommandInterface]
	):
		return null

	message.from_payload(payload)
	if not message.validate().is_empty():
		return null

	return message
```

Validation must also check gameplay rules:

- The match is running.
- The sender is an active, undefeated player.
- The sender owns every commanded entity.
- All referenced entities exist and are alive.
- Targets are valid and visible when required.
- The unit or structure supports the command.
- Resources, energy, fleet capacity, and prerequisites are available.
- Placement, queue size, and research restrictions are satisfied.

## Command Processing Rules

1. The sender creates a unique `command_id`.
2. Multiplayer code wraps the payload in a unique Message Envelope.
3. The Message Factory validates the raw `Dictionary` and creates a typed
   interface implementation.
4. The Command Handler authenticates the sender and validates gameplay rules.
5. The server sends `COMMAND_RESULT`.
6. Accepted commands enter the correct unit, building, or research queue.
7. The Simulation Core processes commands on fixed ticks.
8. State changes are distributed through `STATE_DELTA`.
9. Important outcomes are distributed through `SIMULATION_EVENTS`.

Duplicate `message_id` values must be ignored. Repeating an already processed
`command_id` must return its original result without applying it twice.

## Local And AI Use

Human input, AI, and multiplayer submit the same interface-backed message
objects:

```gdscript
# Human input
command_handler.submit_command(local_player_id, move_command)

# AI
command_handler.submit_command(ai_player_id, move_command)

# Multiplayer server after authenticating the peer
var received_command := MessageFactory.create(
	envelope.message_type,
	envelope.payload
)
command_handler.submit_command(peer_player_id, received_command)
```

This keeps single-player, AI, multiplayer, tutorials, and automated tests on the
same authoritative command path.
