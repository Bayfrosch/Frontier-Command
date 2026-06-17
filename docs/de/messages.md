# Frontier Command Nachrichtenprotokoll

## Aktuelle Implementierung

Die implementierten Nachrichten liegen unter `scripts/messages` und sind C#-
Godot-`RefCounted`-Klassen. Dieses Dokument beschreibt diese Implementierung,
nicht ein zukuenftiges oder theoretisches Wire-Protokoll.

Jede konkrete Nachrichtenklasse leitet von `MessageBase` ab, stellt ein
`message_type` bereit und implementiert:

```csharp
Godot.Collections.Dictionary to_payload();
void from_payload(Godot.Collections.Dictionary payload);
string[] validate();
```

Gameplay-Befehlsklassen implementieren zusaetzlich das Marker-Interface
`CommandInterface`. Befehle mit `queue_mode` implementieren ausserdem das
Marker-Interface `QueueableCommandInterface`.

Die `IMPLEMENTS`-Arrays der Nachrichtenklassen enthalten Interface-Namen fuer
Runtime-Introspection. Eine GDScript-Interface-Addon-Implementierung gibt es im
aktuellen Code aber nicht.

## Serialisierungsregeln

- Nachrichtenklassen serialisieren nach `Godot.Collections.Dictionary`.
- ID-Werte sind `string`.
- Nachrichtennamen und symbolische Status-/Reason-Werte sind `StringName`.
- Positionen sind `Vector2`.
- ID-Listen sind C#-`string[]`, serialisiert als Godot-String-Arrays.
- Strukturierte Listen sind `Godot.Collections.Array<Godot.Collections.Dictionary>`.
- Byte-Daten sind `byte[]`.
- Enums werden als `int`-Werte serialisiert.
- `from_payload()` nutzt tolerante Helper-Konvertierungen und Default-Werte.
- `validate()` prueft nur die unten aufgefuehrten implementierten Strukturregeln.
- Keine Message Factory, kein roher Variant-Schema-Validator und kein
  Gameplay-Regel-Validator ist in `scripts/messages` implementiert.

## Nachrichten-IDs Und Enums

`GameMessages.cs` definiert diese Nachrichten-IDs:

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

Implementierte Enum-Werte:

```csharp
MatchMode: ONE_VS_ONE, FREE_FOR_ALL, TEAM
QueueMode: REPLACE, APPEND
UnitStance: AGGRESSIVE, DEFENSIVE, HOLD_FIRE
OutpostSpecialization: INDUSTRIAL, MILITARY, RESEARCH
MatchPhase: LOBBY, RUNNING, PAUSED, ENDED
VisibilityState: HIDDEN, FOGGED, VISIBLE
MovementCategory: GROUND, AIR, NAVAL
```

## Nachrichten-Envelope

`MessageEnvelope` ist keine `MessageBase`-Unterklasse, verwendet aber dasselbe
Muster mit `to_payload()`, `from_payload()` und `validate()`.

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

Die Validierung verlangt Protokollversion `1`, eine nicht leere `message_id`,
ein nicht leeres `message_type` und ein nicht negatives `sent_at_unix_ms`. Der
aktuelle Serializer nimmt `sender_player_id` auf, wenn das Feld gesetzt ist.

## Gemeinsame Befehlsfelder

Alle implementierten Gameplay-Befehle serialisieren:

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
}
```

Die Validierung verlangt im Allgemeinen eine nicht leere `command_id` und ein
nicht negatives `issued_at_tick`. Queueable-Befehle serialisieren ausserdem:

```csharp
{
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
}
```

Die Queueable-Validierung akzeptiert nur `REPLACE` oder `APPEND`.

`MoveUnitsMessage` hat ein `player_id`-Feld im C#-Basisklassen-Konstruktor,
aber `player_id` wird von `to_payload()` nicht serialisiert und von
`from_payload()` nicht geladen.

## Verbindungsnachrichten

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

Die Validierung verlangt `client_version` und `player_name`.

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

Die Validierung verlangt alle oben genannten Felder. `protocol_version` muss
mindestens `1` sein, und `server_time_unix_ms` darf nicht negativ sein.

### PING

```csharp
{
    "client_time_unix_ms": 0,
}
```

Die Validierung verlangt ein nicht negatives `client_time_unix_ms`.

### PONG

```csharp
{
    "client_time_unix_ms": 0,
    "server_time_unix_ms": 0,
}
```

Die Validierung verlangt, dass beide Zeiten nicht negativ sind.

### DISCONNECT

```csharp
{
    "reason": "client_quit",
}
```

Gueltige Reasons sind `client_quit`, `timeout`, `kicked`, `server_shutdown` und
`protocol_error`.

## Lobby- Und Match-Nachrichten

### MATCH_SETTINGS

`MatchSettingsMessage` ist eine implementierte konkrete Nachricht mit
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

Defaults sind `max_players = 2`, `map_seed = "seed"`,
`map_template = "standard_1v1"`, `map_width_tiles = 1`,
`map_height_tiles = 1` und `allow_pause = true`. Die Validierung verlangt nicht
leere Strings und positive Integer fuer Groessen und Spieleranzahl.

### CREATE_MATCH

```csharp
{
    "settings": {},
}
```

Die Validierung delegiert an `MatchSettingsMessage.validate()`, nachdem das
`settings`-Dictionary geladen wurde.

### UPDATE_MATCH_SETTINGS

`UpdateMatchSettingsMessage` erbt von `CreateMatchMessage`, setzt
`message_type` auf `update_match_settings` und verwendet dieselbe Payload und
Validierung.

### JOIN_MATCH

```csharp
{
    "match_id": "match-id",
    "as_observer": false,
}
```

Die Validierung verlangt `match_id`.

### LEAVE_MATCH

```csharp
{}
```

### SET_PLAYER_READY

```csharp
{
    "is_ready": true,
    "faction_id": "frontier_coalition",

    // Optional when not empty:
    "team_id": "team-1",
}
```

Die Validierung verlangt `faction_id`.

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

Die Validierung verlangt eine nicht leere `lobby_id`, eine nicht leere
`host_player_id`, ein nicht leeres Players-Array, Match-Setting-Keys in
`settings` und jeden oben aufgefuehrten Player-Key. In der aktuellen
Implementierung wird `team_id` als nicht negativer int validiert, nicht als
optionaler String.

### START_MATCH

```csharp
{}
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

Die Validierung verlangt `match_id`, ein nicht negatives `start_tick`, ein
positives `tick_rate` und `map_seed`.

### PAUSE_MATCH

```csharp
{}
```

Es gibt keine implementierten `resume_match`- oder `surrender`-Nachrichten.

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

Die Validierung verlangt ein nicht negatives `end_tick` und eine gueltige
Reason. Gueltige Reasons sind `victory_conditions`, `surrender`,
`all_opponents_disconnected` und `admin_ended`. Winner- und Defeated-Arrays
werden von der implementierten Validierung nicht verlangt.

## Einheitenbefehle

### MOVE_UNITS

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1", "unit-2" },
    "destination": new Vector2(100.0f, 250.0f),
    "formation": "rectangle",
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, mindestens eine Unit-ID und `formation` gleich `none` oder
`rectangle`.

### ATTACK_MOVE_UNITS

Gleiche Payload und Validierung wie `MOVE_UNITS`, mit
`message_type = attack_move_units`.

### ATTACK_TARGET

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1", "unit-2" },
    "target_id": "enemy-1",
}
```

Der implementierte Key ist `target_id`, nicht `target_entity_id`.
`AttackTargetMessage` ueberschreibt `validate()` aktuell nicht und erbt daher
die Default-No-op-Validierung von `MessageBase`.

### STOP_UNITS

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1", "unit-2" },
}
```

Es gibt kein implementiertes `clear_queue`-Feld. `StopUnitsMessage`
ueberschreibt `validate()` aktuell nicht.

### HOLD_POSITION_UNITS

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1" },
    "enabled": true,
}
```

Die implementierte Nachrichten-ID ist `hold_position_units`. Die Validierung
verlangt eine Command-ID, einen nicht negativen Tick und mindestens eine
Unit-ID.

### PATROL_UNITS

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "unit_ids": new string[] { "unit-1" },
    "destination": new Vector2(100.0f, 250.0f),
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode und mindestens eine Unit-ID.

### SET_UNIT_STANCE

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "unit_ids": new string[] { "unit-1" },
    "stance": (int)GameMessages.UnitStance.DEFENSIVE,
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, mindestens
eine Unit-ID und eine gueltige `UnitStance`.

### USE_ABILITY

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "caster_entity_ids": new string[] { "unit-1" },
    "ability_id": "ability-id",

    // Optional; at most one is valid:
    "target_entity_id": "enemy-1",
    "target_position": new Vector2(100.0f, 250.0f),
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, mindestens eine Caster-ID und `ability_id`. Sie lehnt es
ab, beide Ziel-Formen gleichzeitig zu verwenden, und verlangt, dass
`target_position` bei Vorhandensein ein `Vector2` ist.

### GATHER_RESOURCES

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "harvester_entity_ids": new string[] { "harvester-1" },
    "resource_field_entity_id": "resource-field-1",

    // Optional when not empty:
    "refinery_entity_id": "refinery-1",
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, mindestens eine Harvester-ID und
`resource_field_entity_id`.

## Bau- Und Support-Befehle

### BUILD_STRUCTURE

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "construction_unit_id": "worker-1",
    "building_definition_id": "basic_generator",
    "position": new Vector2(100.0f, 250.0f),
    "rotation_radians": 0.0f,
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, `construction_unit_id` und `building_definition_id`.

### CANCEL_CONSTRUCTION

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "construction_site_id": "construction-site-1",
}
```

Die Validierung verlangt alle drei obigen Felder, mit einem nicht negativen
Tick.

### REPAIR_TARGET

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "repair_unit_ids": new string[] { "worker-1" },
    "target_entity_id": "building-1",
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, mindestens eine Repair-Unit-ID und `target_entity_id`.

### CAPTURE_TARGET

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "queue_mode": (int)GameMessages.QueueMode.REPLACE,
    "envoy_unit_ids": new string[] { "envoy-1" },
    "target_entity_id": "outpost-1",
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick, einen
gueltigen Queue Mode, mindestens eine Envoy-ID und `target_entity_id`.

### UPGRADE_STRUCTURE

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "structure_entity_id": "building-1",
    "upgrade_definition_id": "advanced_production",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

### CANCEL_STRUCTURE_UPGRADE

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "structure_entity_id": "building-1",
    "upgrade_queue_item_id": "queue-item-1",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

## Produktionsbefehle

### TRAIN_UNITS

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "unit_definition_id": "main_battle_unit",
    "quantity": 3,
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick,
`producer_entity_id`, `unit_definition_id` und `quantity > 0`.

### CANCEL_PRODUCTION

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "queue_item_id": "queue-item-1",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

### REORDER_PRODUCTION

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "producer_entity_id": "factory-1",
    "queue_item_id": "queue-item-1",
    "new_index": 0,
}
```

Die Validierung verlangt alle obigen Felder sowie nicht negative Werte fuer
`issued_at_tick` und `new_index`.

### SET_RALLY_POINT

```csharp
{
    "command_id": "command-id",
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
    "command_id": "command-id",
    "issued_at_tick": 120,
    "producer_entity_ids": new string[] { "factory-1" },
    "target": {
        "kind": "entity",
        "entity_id": "entity-1",
    },
}
```

`target` darf `null`/Nil sein, um den Rally Point zu loeschen. Die Validierung
verlangt eine Command-ID, einen nicht negativen Tick und mindestens eine
Producer-ID. Wenn target nicht Nil ist, muss es ein Dictionary mit
`kind = position` und einer Vector2-`position` oder `kind = entity` und nicht
leerer `entity_id` sein.

## Forschungs- Und Fortschrittsbefehle

Diese Dateien liegen aktuell unter `scripts/messages/commands/research`,
einschliesslich `ChooseCapitalUpgradeMessage`.

### START_RESEARCH

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "research_structure_id": "research-center-1",
    "research_id": "research-id",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

### CANCEL_RESEARCH

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "research_structure_id": "research-center-1",
    "research_queue_item_id": "queue-item-1",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

### CHOOSE_CAPITAL_UPGRADE

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "capital_unit_entity_id": "capital-unit-1",
    "upgrade_definition_id": "capital-upgrade-id",
}
```

Die Validierung verlangt alle obigen Felder, mit einem nicht negativen Tick.

### SPECIALIZE_OUTPOST

```csharp
{
    "command_id": "command-id",
    "issued_at_tick": 120,
    "outpost_entity_id": "outpost-1",
    "specialization": (int)GameMessages.OutpostSpecialization.INDUSTRIAL,
}
```

Die Validierung verlangt eine Command-ID, einen nicht negativen Tick,
`outpost_entity_id` und eine gueltige `OutpostSpecialization`.

## Befehlsergebnis

### Accepted

```csharp
{
    "command_id": "command-id",
    "status": "accepted",
    "accepted_at_tick": 121,
}
```

### Rejected

```csharp
{
    "command_id": "command-id",
    "status": "rejected",
    "rejection": {
        "code": "not_owner",
        "details": "The player does not own every selected unit.",
        "entity_ids": new string[] { "unit-1" },
    },
}
```

Die Validierung verlangt `command_id`, `status` gleich `accepted` oder
`rejected`, `accepted_at_tick >= 0` fuer akzeptierte Ergebnisse und ein nicht
leeres Rejection-Dictionary mit gueltigem `code` fuer abgelehnte Ergebnisse.

Implementierte Rejection-Codes:

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

## Zustandssynchronisierung

Die aktuellen State-Nachrichten transportieren Dictionaries und
Dictionary-Arrays. Es gibt keine implementierten typisierten
`PlayerState`-, `EntityState`-, `UnitState`-, `BuildingState`-, `OutpostState`-
oder `ResourceFieldState`-Nachrichtenklassen.

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

Die Validierung verlangt `match_id`, einen nicht negativen `tick`, eine
gueltige `MatchPhase`, ein nicht negatives `match_time_ms` und `checksum`.
Leere Player-/Entity-Arrays sind nach der implementierten Validierung erlaubt.

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

Die Validierung verlangt ein nicht negatives `from_tick`, `to_tick >= from_tick`
und `checksum`.

### RESYNC_REQUEST

```csharp
{
    "client_tick": 120,
    "client_checksum": "checksum",
    "reason": "checksum_mismatch",
}
```

Gueltige Reasons sind `missing_delta`, `checksum_mismatch` und `reconnect`.
Die Validierung verlangt einen nicht negativen Tick, `client_checksum` und eine
gueltige Reason.

### RESYNC_RESPONSE

```csharp
{
    "snapshot": {},
}
```

Die Validierung verlangt ein nicht leeres Snapshot-Dictionary.

## Simulation Events

`SimulationEventsMessage` transportiert Event-Dictionaries ohne typisierte
event-spezifische Nachrichtenklassen.

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

Die Validierung verlangt, dass der Batch-`tick` nicht negativ ist. Fuer jedes
Event-Dictionary verlangt sie eine nicht leere `event_id`, ein nicht leeres
`event_type` und einen `tick`-Key, dessen Wert nicht negativ ist. Es sind keine
event-spezifischen Feldanforderungen implementiert.

## Generischer Fehler

```csharp
{
    "code": "match_not_found",
    "retryable": false,

    // Optional when not empty:
    "details": "details",
}
```

Die Validierung verlangt, dass `code` einer der folgenden Werte ist:

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

## Implementierter Validierungsumfang

Die aktuelle Nachrichtenschicht validiert nur die lokale Nachrichtenform. Sie
implementiert aktuell nicht:

- Behandlung doppelter `message_id`- oder `command_id`-Werte.
- Authentifizierung oder Sender-Ableitung.
- Ownership-Pruefungen.
- Match-Phase-Pruefungen ausser einfacher Enum-Validierung in Snapshots.
- Fog-of-war-Filterung.
- Entity-Existenz, Zielgueltigkeit, Platzierung, Ressourcen, Voraussetzungen,
  Queue-Kapazitaet oder andere Gameplay-Regeln.
- Eine konkrete `MessageFactory`.

Diese Regeln koennen in Server-, Command-Handler-, Simulations- oder kuenftigem
Factory-Code liegen, sind aber nicht durch die hier dokumentierten
Nachrichtenklassen implementiert.
