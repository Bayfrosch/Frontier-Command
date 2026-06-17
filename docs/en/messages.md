# Frontier Command Message Protocol

## Current Implementation

The implemented messages live under `scripts/messages` and are C# Godot
`RefCounted` classes. This document describes that implementation, not a future
or theoretical wire protocol.

Each concrete message class derives from `MessageBase`, exposes a
`message_type`, and implements:

```csharp
Godot.Collections.Dictionary to_payload();
void from_payload(Godot.Collections.Dictionary payload);
string[] validate();
```

Gameplay command classes also implement the marker interface
`CommandInterface`. Commands that include a `queue_mode` also implement the
marker interface `QueueableCommandInterface`.

The `IMPLEMENTS` arrays on message classes contain interface names for runtime
introspection, but there is no GDScript interface-addon implementation in the
current code.

## Serialization Rules

- Message classes serialize to `Godot.Collections.Dictionary`.
- ID values are `string`.
- Message names and symbolic statuses/reasons are `StringName`.
- Positions are `Vector2`.
- Lists of IDs are C# `string[]`, serialized as Godot string arrays.
- Structured lists are `Godot.Collections.Array<Godot.Collections.Dictionary>`.
- Byte data is `byte[]`.
- Enums are serialized as `int` values.
- `from_payload()` uses permissive helper conversions and default values.
- `validate()` checks only the implemented structural rules listed below.
- No message factory, raw Variant schema validator, or gameplay-rule validator
  is implemented in `scripts/messages`.

## Message IDs And Enums

`GameMessages.cs` defines these message IDs:

```csharp
client_hello
server_hello
ping
pong
disconnect
match_settings
create_match
join_match
leave_match
set_player_ready
update_match_settings
lobby_state
start_match
match_started
pause_match
match_ended
move_units
stop_units
hold_position_units
patrol_units
set_unit_stance
use_ability
gather_resources
attack_move_units
attack_target
build_structure
cancel_construction
repair_target
capture_target
upgrade_structure
cancel_structure_upgrade
train_units
cancel_production
reorder_production
set_rally_point
start_research
cancel_research
choose_capital_upgrade
specialize_outpost
command_result
state_snapshot
state_delta
resync_request
resync_response
simulation_events
generic_error
```

Implemented enum values:

```csharp
MatchMode: ONE_VS_ONE, FREE_FOR_ALL, TEAM
QueueMode: REPLACE, APPEND
UnitStance: AGGRESSIVE, DEFENSIVE, HOLD_FIRE
OutpostSpecialization: INDUSTRIAL, MILITARY, RESEARCH
MatchPhase: LOBBY, RUNNING, PAUSED, ENDED
VisibilityState: HIDDEN, FOGGED, VISIBLE
MovementCategory: GROUND, AIR, NAVAL
```

## Message Envelope

`MessageEnvelope` is not a `MessageBase` subclass, but has the same
`to_payload()`, `from_payload()`, and `validate()` pattern.

Payload:

```csharp
{
    "protocol_version": 1,
    "message_id": "message-id",
    "message_type": GameMessages.MOVE_UNITS,
    "sent_at_unix_ms": 0,
    "payload": new Dictionary(),

    // Optional when not empty:
    "match_id": "match-id",
    "sender_player_id": "player-id",
    "request_id": "request-id",
}
```

Validation requires protocol version `1`, non-empty `message_id`, non-empty
`message_type`, and non-negative `sent_at_unix_ms`. The current serializer does
include `sender_player_id` when the field is set.

## Shared Command Fields

All implemented gameplay commands serialize:

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
}
```

Validation generally requires a non-empty `player_id` and a non-negative
`issued_at_tick`. Queueable commands also serialize:

```csharp
{
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
}
```

Queueable validation accepts only `REPLACE` or `APPEND`.

Player-driven lobby and match request messages also carry `player_id` where the
implementation needs to identify the acting player.

## Connection Messages

### CLIENT_HELLO

Payload:

```csharp
{
    "client_version": "0.1.0",
    "player_name": "Player",

    // Optional when not empty:
    "reconnect_token": "token",
}
```

Validation requires `client_version` and `player_name`.

### SERVER_HELLO

Payload:

```csharp
{
    "server_version": "0.1.0",
    "protocol_version": 1,
    "player_id": "player-id",
    "reconnect_token": "token",
    "server_time_unix_ms": 0,
}
```

Validation requires all fields above. `protocol_version` must be at least `1`
and `server_time_unix_ms` must be non-negative.

### PING

```csharp
{
    "client_time_unix_ms": 0,
}
```

Validation requires a non-negative `client_time_unix_ms`.

### PONG

```csharp
{
    "client_time_unix_ms": 0,
    "server_time_unix_ms": 0,
}
```

Validation requires both times to be non-negative.

### DISCONNECT

```csharp
{
    "reason": "client_quit",
}
```

Valid reasons are `client_quit`, `timeout`, `kicked`, `server_shutdown`, and
`protocol_error`.

## Lobby And Match Messages

### MATCH_SETTINGS

`MatchSettingsMessage` is an implemented concrete message with
`message_type = match_settings`.

```csharp
{
    "match_name": "Frontier Match",
    "max_players": 2,
    "map_seed": "seed",
    "map_template": "standard_1v1",
    "map_width_tiles": 1,
    "map_height_tiles": 1,
    "allow_pause": true,
}
```

Defaults are `max_players = 2`, `map_seed = "seed"`,
`map_template = "standard_1v1"`, `map_width_tiles = 1`,
`map_height_tiles = 1`, and `allow_pause = true`. Validation requires non-empty
strings and positive integer sizes/player count.

### CREATE_MATCH

```csharp
{
    "player_id": "player-id",
    "settings": {},
}
```

Validation requires `player_id` and delegates to
`MatchSettingsMessage.validate()` after loading the `settings` dictionary.

### UPDATE_MATCH_SETTINGS

`UpdateMatchSettingsMessage` inherits from `CreateMatchMessage`, changes
`message_type` to `update_match_settings`, and uses the same payload and
validation.

### JOIN_MATCH

```csharp
{
    "player_id": "player-id",
    "match_id": "match-id",
    "as_observer": false,
}
```

Validation requires `player_id` and `match_id`.

### LEAVE_MATCH

```csharp
{
    "player_id": "player-id",
}
```

### SET_PLAYER_READY

```csharp
{
    "player_id": "player-id",
    "is_ready": true,
    "faction_id": "frontier_coalition",

    // Optional when not empty:
    "team_id": "team-1",
}
```

Validation requires `player_id` and `faction_id`.

### LOBBY_STATE

```csharp
{
    "lobby_id": "lobby-id",
    "host_player_id": "player-id",
    "settings": {},
    "players": [
        {
            "player_id": "player-id",
            "display_name": "Player",
            "is_ai": false,
            "is_ready": true,
            "team_id": 0,
            "faction": "frontier_coalition",
            "color": Colors.Red,
        },
    ],
}
```

Validation requires non-empty `lobby_id`, non-empty `host_player_id`, a
non-empty players array, match-setting keys inside `settings`, and each listed
player key above. In the current implementation `team_id` is validated as a
non-negative int, not an optional string.

### START_MATCH

```csharp
{
    "player_id": "player-id",
}
```

### MATCH_STARTED

```csharp
{
    "match_id": "match-id",
    "start_tick": 0,
    "tick_rate": 20,
    "map_seed": "seed",

    // Optional when not empty:
    "local_player_id": "player-id",
}
```

Validation requires `match_id`, non-negative `start_tick`, positive `tick_rate`,
and `map_seed`.

### PAUSE_MATCH

```csharp
{
    "player_id": "player-id",
}
```

There are no implemented `resume_match` or `surrender` messages.

### MATCH_ENDED

```csharp
{
    "end_tick": 10000,
    "winner_player_ids": new string[] { "player-1" },
    "defeated_player_ids": new string[] { "player-2" },
    "reason": "victory_conditions",

    // Optional when not empty:
    "winner_team_id": "team-1",
}
```

Validation requires non-negative `end_tick` and a valid reason. Valid reasons
are `victory_conditions`, `surrender`, `all_opponents_disconnected`, and
`admin_ended`. Winner and defeated arrays are not required by validation.

## Unit Commands

### MOVE_UNITS

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1", "unit-2" },
    "destination": new Vector2(100.0f, 250.0f),
    "formation": "rectangle",
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, at least
one unit id, and `formation` equal to `none` or `rectangle`.

### ATTACK_MOVE_UNITS

Same payload and validation as `MOVE_UNITS`, with
`message_type = attack_move_units`.

### ATTACK_TARGET

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1", "unit-2" },
    "target_id": "enemy-1",
}
```

The implemented key is `target_id`, not `target_entity_id`. Validation requires
`player_id`, a non-negative tick, valid queue mode, at least one unit id, and
`target_id`.

### STOP_UNITS

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1", "unit-2" },
}
```

There is no implemented `clear_queue` field. Validation requires `player_id`, a
non-negative tick, and at least one unit id.

### HOLD_POSITION_UNITS

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1" },
    "enabled": true,
}
```

The implemented message ID is `hold_position_units`. Validation requires a
non-negative tick and at least one unit id.

### PATROL_UNITS

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1" },
    "destination": new Vector2(100.0f, 250.0f),
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, and at
least one unit id.

### SET_UNIT_STANCE

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1" },
    "stance": (int)GameMessages.UnitStance.DEFENSIVE,
}
```

Validation requires `player_id`, a non-negative tick, at least one unit id, and a
valid `UnitStance`.

### USE_ABILITY

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "caster_entity_ids": new string[] { "unit-1" },
    "ability_id": "ability-id",

    // Optional; at most one is valid:
    "target_entity_id": "enemy-1",
    "target_position": new Vector2(100.0f, 250.0f),
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, at least
one caster id, and `ability_id`. It rejects using both target forms and requires
`target_position` to be a `Vector2` when present.

### GATHER_RESOURCES

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "harvester_entity_ids": new string[] { "harvester-1" },
    "resource_field_entity_id": "resource-field-1",

    // Optional when not empty:
    "refinery_entity_id": "refinery-1",
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, at least
one harvester id, and `resource_field_entity_id`.

## Construction And Support Commands

### BUILD_STRUCTURE

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "construction_unit_id": "worker-1",
    "building_definition_id": "basic_generator",
    "position": new Vector2(100.0f, 250.0f),
    "rotation_radians": 0.0f,
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode,
`construction_unit_id`, and `building_definition_id`.

### CANCEL_CONSTRUCTION

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "construction_site_id": "construction-site-1",
}
```

Validation requires all three fields above, with a non-negative tick.

### REPAIR_TARGET

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "repair_unit_ids": new string[] { "worker-1" },
    "target_entity_id": "building-1",
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, at least
one repair unit id, and `target_entity_id`.

### CAPTURE_TARGET

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "envoy_unit_ids": new string[] { "envoy-1" },
    "target_entity_id": "outpost-1",
}
```

Validation requires `player_id`, a non-negative tick, valid queue mode, at least
one envoy id, and `target_entity_id`.

### UPGRADE_STRUCTURE

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "structure_entity_id": "building-1",
    "upgrade_definition_id": "advanced_production",
}
```

Validation requires all fields above, with a non-negative tick.

### CANCEL_STRUCTURE_UPGRADE

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "structure_entity_id": "building-1",
    "upgrade_queue_item_id": "queue-item-1",
}
```

Validation requires all fields above, with a non-negative tick.

## Production Commands

### TRAIN_UNITS

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "unit_definition_id": "main_battle_unit",
    "quantity": 3,
}
```

Validation requires `player_id`, a non-negative tick, `producer_entity_id`,
`unit_definition_id`, and `quantity > 0`.

### CANCEL_PRODUCTION

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "queue_item_id": "queue-item-1",
}
```

Validation requires all fields above, with a non-negative tick.

### REORDER_PRODUCTION

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "queue_item_id": "queue-item-1",
    "new_index": 0,
}
```

Validation requires all fields above, with non-negative `issued_at_tick` and
`new_index`.

### SET_RALLY_POINT

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "producer_entity_ids": new string[] { "factory-1" },
    "target": {
        "kind": "position",
        "position": new Vector2(100.0f, 250.0f),
    },
}
```

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "producer_entity_ids": new string[] { "factory-1" },
    "target": {
        "kind": "entity",
        "entity_id": "entity-1",
    },
}
```

`target` may be `null`/Nil to clear the rally point. Validation requires a
non-negative tick and at least one producer id. When target is not
Nil, it must be a dictionary with `kind = position` and a Vector2 `position`, or
`kind = entity` and non-empty `entity_id`.

## Research And Progression Commands

These files are currently under `scripts/messages/commands/research`, including
`ChooseCapitalUpgradeMessage`.

### START_RESEARCH

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "research_structure_id": "research-center-1",
    "research_id": "research-id",
}
```

Validation requires all fields above, with a non-negative tick.

### CANCEL_RESEARCH

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "research_structure_id": "research-center-1",
    "research_queue_item_id": "queue-item-1",
}
```

Validation requires all fields above, with a non-negative tick.

### CHOOSE_CAPITAL_UPGRADE

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "capital_unit_entity_id": "capital-unit-1",
    "upgrade_definition_id": "capital-upgrade-id",
}
```

Validation requires all fields above, with a non-negative tick.

### SPECIALIZE_OUTPOST

```csharp
{
    "player_id": "player-id",
    "issued_at_tick": 120,
    "outpost_entity_id": "outpost-1",
    "specialization": (int)GameMessages.OutpostSpecialization.INDUSTRIAL,
}
```

Validation requires `player_id`, a non-negative tick, `outpost_entity_id`, and a
valid `OutpostSpecialization`.

## Command Result

### Accepted

```csharp
{
    "status": "accepted",
    "accepted_at_tick": 121,
}
```

### Rejected

```csharp
{
    "status": "rejected",
    "rejection": {
        "code": "not_owner",
        "details": "The player does not own every selected unit.",
        "entity_ids": new string[] { "unit-1" },
    },
}
```

Validation requires `status` equal to `accepted` or `rejected`,
`accepted_at_tick >= 0` for accepted results, and a non-empty rejection
dictionary with a valid `code` for rejected results.

Implemented rejection codes:

```csharp
malformed_command
player_not_in_match
player_defeated
not_owner
entity_not_found
entity_destroyed
invalid_target
target_not_visible
unsupported_command
invalid_placement
unreachable_destination
insufficient_materials
insufficient_energy
fleet_capacity_exceeded
missing_prerequisite
research_locked
already_researched
doctrine_already_chosen
queue_full
match_not_running
rate_limited
```

## State Synchronization

The current state messages carry dictionaries and dictionary arrays. There are
no implemented typed `PlayerState`, `EntityState`, `UnitState`, `BuildingState`,
`OutpostState`, or `ResourceFieldState` message classes.

### STATE_SNAPSHOT

```csharp
{
    "match_id": "match-id",
    "tick": 120,
    "phase": (int)GameMessages.MatchPhase.RUNNING,
    "match_time_ms": 6000,
    "players": new Array<Dictionary>(),
    "entities": new Array<Dictionary>(),
    "explored_map_data": new byte[] {},
    "checksum": "checksum",
}
```

Validation requires `match_id`, non-negative `tick`, valid `MatchPhase`,
non-negative `match_time_ms`, and `checksum`. Empty player/entity arrays are
allowed by the implemented validation.

### STATE_DELTA

```csharp
{
    "from_tick": 120,
    "to_tick": 121,
    "changed_players": new Array<Dictionary>(),
    "upserted_entities": new Array<Dictionary>(),
    "removed_entity_ids": new string[] {},
    "checksum": "checksum",

    // Optional when not empty:
    "explored_map_patch": new byte[] {},
}
```

Validation requires non-negative `from_tick`, `to_tick >= from_tick`, and
`checksum`.

### RESYNC_REQUEST

```csharp
{
    "client_tick": 120,
    "client_checksum": "checksum",
    "reason": "checksum_mismatch",
}
```

Valid reasons are `missing_delta`, `checksum_mismatch`, and `reconnect`.
Validation requires a non-negative tick, `client_checksum`, and a valid reason.

### RESYNC_RESPONSE

```csharp
{
    "snapshot": {},
}
```

Validation requires a non-empty snapshot dictionary.

## Simulation Events

`SimulationEventsMessage` carries event dictionaries without typed
event-specific message classes.

```csharp
{
    "tick": 121,
    "events": [
        {
            "event_id": "event-id",
            "event_type": "unit_created",
            "tick": 121,
        },
    ],
}
```

Validation requires the batch `tick` to be non-negative. For each event
dictionary, validation requires non-empty `event_id`, non-empty `event_type`,
and an event `tick` key whose value is non-negative. No event-specific field
requirements are implemented.

## Generic Error

```csharp
{
    "code": "match_not_found",
    "retryable": false,

    // Optional when not empty:
    "details": "details",
}
```

Validation requires `code` to be one of:

```csharp
unsupported_protocol_version
authentication_failed
permission_denied
match_not_found
match_full
invalid_match_settings
player_not_ready
request_timeout
internal_server_error
```

## Implemented Validation Scope

The current message layer validates local message shape only. It does not
currently implement:

- Duplicate `message_id` handling.
- Authentication or sender derivation.
- Ownership checks.
- Match phase checks beyond simple enum validation in snapshots.
- Fog-of-war filtering.
- Entity existence, target validity, placement, resources, prerequisites, queue
  capacity, or other gameplay rules.
- A concrete `MessageFactory`.

Those rules may belong in server, command handler, simulation, or future factory
code, but they are not implemented by the message classes documented here.
