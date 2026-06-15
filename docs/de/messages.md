# Frontier-Command-Nachrichtenprotokoll

## Zweck

Dieses Dokument definiert die GDScript-Nachrichten, die zwischen Client,
Command Handler, Simulation Core, KI und Mehrspielerserver ausgetauscht werden.

Befehle beschreiben eine Absicht. Der maßgebliche Simulation Core validiert
Befehle, verändert den Spielzustand und gibt Ergebnisse aus. Clients und KI
verändern den maßgeblichen Zustand niemals direkt.

Frontier Command verwendet den
[`GDScript-Interfaces-addon`](https://github.com/Rito13/GDScript-Interfaces-addon),
um im Editor und zur Laufzeit geprüfte Interfaces zu definieren. Konkrete
Nachrichten sind typisierte `RefCounted`-Klassen. Sie werden nur beim
Überschreiten der Netzwerkgrenze in `Dictionary`-Werte umgewandelt.

Die Bibliothek prüft erforderliche Eigenschaften, Methoden, Signale und deren
deklarierte Typen. Eine explizite Nachrichtenprüfung bleibt für erlaubte Werte
und Gameplay-Regeln erforderlich.

## GDScript-Konventionen

- Das Projekt ist für Godot 4.6 ausgelegt.
- Nachrichtenklassen implementieren `MessageInterface`.
- Gameplay-Befehlsklassen implementieren zusätzlich `CommandInterface`.
- Warteschlangenfähige Befehlsklassen implementieren zusätzlich `QueueableCommandInterface`.
- Nachrichten- und Ereignisnamen verwenden `StringName`.
- IDs verwenden `String`.
- Positionen verwenden Godots `Vector2`.
- Listen von Entitäts-IDs verwenden `PackedStringArray`.
- Strukturierte Listen verwenden `Array[Dictionary]`.
- Optionale Schlüssel dürfen fehlen. `null` wird nur verwendet, um einen Wert ausdrücklich zu löschen.
- Zeitwerte sind Millisekunden, sofern nicht anders angegeben.
- Die Simulationszeit verwendet einen ganzzahligen `tick`.
- Nachrichtenschlüssel verwenden gemäß den GDScript-Konventionen `snake_case`.
- `to_payload()` darf nur Variant-kompatible Werte zurückgeben.
- `from_payload()` wird erst aufgerufen, nachdem die Message Factory die Rohdatentypen geprüft hat.
- Der Client sendet niemals berechnete Kosten, Schaden, Fortschritt oder Ergebnisse.
- An einen Client gesendete Zustände und Ereignisse müssen nach dem Kriegsnebel gefiltert werden.

## Interface-Bibliothek

Installiere das Addon unter `res://addons/gdscript-interfaces/`, aktiviere das
Plugin und lasse dessen `InterfacesAutoload` aktiviert. Interface-Klassen
erweitern `BasicInterface`. Implementierende Klassen geben die Interface-Namen
in der großgeschriebenen Konstante `IMPLEMENTS` an:

```gdscript
const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
]
```

Prüfe ein Objekt, bevor es angenommen wird:

```gdscript
if not Interfaces.implements(message, &"MessageInterface"):
	push_error("Object does not implement MessageInterface.")
	return
```

## Nachrichten-Interfaces

### MessageInterface

Jede Nachricht implementiert:

```gdscript
# Interface
class_name MessageInterface
extends BasicInterface

var message_type: StringName

func to_payload() -> Dictionary:
	return {}

func from_payload(_payload: Dictionary) -> void:
	pass

func validate() -> PackedStringArray:
	return PackedStringArray()
```

`validate()` gibt bei einer gültigen Nachricht ein leeres Array zurück.
Andernfalls beschreibt jeder Eintrag einen Validierungsfehler.

### CommandInterface

Jeder Gameplay-Befehl implementiert sowohl `MessageInterface` als auch
`CommandInterface`:

```gdscript
# Interface
class_name CommandInterface
extends BasicInterface

var command_id: String
var issued_at_tick: int
```

### QueueableCommandInterface

Befehle, die eine Befehlswarteschlange ersetzen oder erweitern können,
implementieren zusätzlich:

```gdscript
# Interface
class_name QueueableCommandInterface
extends BasicInterface

var queue_mode: int
```

### Beispiel einer konkreten Nachricht

Jede Nachricht ist eine typisierte Klasse. Dies ist die Implementierungsform
für `MOVE_UNITS`:

```gdscript
class_name MoveUnitsMessage
extends RefCounted

const IMPLEMENTS: Array[StringName] = [
	&"MessageInterface",
	&"CommandInterface",
	&"QueueableCommandInterface",
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

Die folgenden Abschnitte dokumentieren die Felder und serialisierte Payload
jeder konkreten Klasse. Verwende eine `.gd`-Datei pro konkreter Nachricht.

## Gemeinsame ID-Typen

GDScript kann keine Aliasse wie `PlayerId` oder `EntityId` erstellen. Diese
Werte werden alle durch `String` dargestellt, sollten aber eindeutig benannt
werden:

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

IDs sind undurchsichtig. Code darf sie nur vergleichen und keine Informationen
aus ihnen ableiten.

## Nachrichtenumschlag

Jede Mehrspielernachricht verwendet diese äußere Struktur:

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

Vorgeschlagener Konstruktor:

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

Der Server leitet `sender_player_id` vom authentifizierten Peer ab. Er darf
keiner vom Client übermittelten Spieler-ID vertrauen.

## Befehlsbasis

Jede Gameplay-Befehlspayload enthält:

```gdscript
{
	"command_id": "unique-command-id", # String
	"issued_at_tick": 120,             # int
}
```

Warteschlangenfähige Befehle enthalten zusätzlich:

```gdscript
{
	"queue_mode": GameMessages.QueueMode.REPLACE, # QueueMode
}
```

`player_id` ist nicht Teil einer Client-Befehlspayload. Der Server ordnet den
Befehl seinem authentifizierten Netzwerk-Peer zu. Lokaler KI-Code übergibt seine
Spieler-ID separat an den Command Handler.

## Verbindungsnachrichten

### CLIENT_HELLO

```gdscript
{
	"client_version": "0.1.0", # String
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
}
```

## Lobby- und Partienachrichten

### Partieeinstellungen

```gdscript
{
	"match_name": "Frontier Match",    # String
	"max_players": 2,                  # int
	"map_seed": "seed",                # String
	"map_template": "standard_1v1",    # String
	"map_width_tiles": 192,            # int
	"map_height_tiles": 192,           # int
	"allow_pause": true,               # bool
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

Nur für den Host verfügbarer Befehl.

```gdscript
{
	"settings": {}, # Match Settings Dictionary
}
```

### LOBBY_STATE

```gdscript
{
	"lobby_id": "lobby-id",         # String
	"host_player_id": "player-id",  # String
	"settings": {},                 # Match Settings Dictionary
	"players": [                    # Array[Dictionary]
		{
			"player_id": "player-id",
			"display_name": "Player",
			"is_ai": false,
			"is_ready": true,
			"team_id": 1,
			"faction": "frontier_coalition",
			"color": Color.RED,
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

Alle verwenden eine leere Payload:

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

Gültige Gründe sind `victory_conditions`, `surrender`,
`all_opponents_disconnected` und `admin_ended`.

## Einheitenbefehle

Jedes folgende Beispiel enthält zusätzlich die Schlüssel der Befehlsbasis.

### MOVE_UNITS

Die maßgebliche Simulation kennt bereits die aktuelle Position jeder Einheit,
daher sendet der Client keine `from`-Position.

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

Gib höchstens eine Zielform an. Lasse beide bei einer Fähigkeit ohne Ziel weg.

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

Sammler wiederholen den Sammel- und Ablieferungszyklus, bis sie angehalten
werden, das Feld erschöpft ist oder keine gültige Raffinerie mehr vorhanden ist.

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

## Bau- und Unterstützungsbefehle

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

## Produktionsbefehle

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

Positionsziel:

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

Entitätsziel:

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

Verwende `"target": null`, um den Sammelpunkt zu löschen.

## Forschungs- und Fortschrittsbefehle

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

## Befehlsergebnis

Jeder Befehl erhält ein anfängliches Ergebnis. Eine Annahme bedeutet, dass der
Befehl beim Absenden gültig war; sie garantiert nicht, dass sein Ziel erreicht
wird.

Angenommen:

```gdscript
{
	"command_id": "command-id",
	"status": &"accepted",
	"accepted_at_tick": 121,
}
```

Abgelehnt:

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

Ablehnungscodes:

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

## Maßgeblicher Zustand

### Spielerzustand

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

### Entitätszustand

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

Nur der zu `kind` passende Untertyp darf vorhanden sein.

### Einheitenzustand

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

### Gebäudezustand

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

Jeder Warteschlangeneintrag verwendet:

```gdscript
{
	"queue_item_id": "queue-item-1", # String
	"definition_id": "definition-id",# String
	"progress": 0.5,                 # float from 0.0 to 1.0
	"state": &"active",              # waiting, active, or paused
}
```

Für Forschung wird `research_id` verwendet; für Gebäudeverbesserungen wird
`upgrade_definition_id` statt `definition_id` verwendet.

### Außenpostenzustand

```gdscript
{
	"capture_progress": 0.0, # float from 0.0 to 1.0

	# Optional:
	"specialization": GameMessages.OutpostSpecialization.INDUSTRIAL,
	"capturing_player_id": "player-id",
}
```

### Ressourcenfeldzustand

```gdscript
{
	"materials_remaining": 200000, # int
	"is_depleted": false,          # bool
}
```

## Zustandssynchronisierung

### STATE_SNAPSHOT

Ein vollständiger, nach Kriegsnebel gefilterter Zustand, der beim Partiestart,
Beitritt eines Beobachters, Wiederverbinden oder Resynchronisieren gesendet wird.

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

`removed_entity_ids` enthält zerstörte Entitäten und gegnerische Entitäten, die
den sichtbaren Bereich verlassen haben. Der Client darf nicht darauf schließen,
welcher Fall eingetreten ist, sofern er nicht zusätzlich ein Ereignis empfängt,
das er kennen darf.

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

## Simulationsereignisse

Wichtige Ergebnisse werden gebündelt gesendet:

```gdscript
{
	"tick": 121,
	"events": [], # Array[Dictionary]
}
```

Jedes Ereignis enthält:

```gdscript
{
	"event_id": "event-id",       # String
	"event_type": &"unit_created",# StringName
	"tick": 121,                  # int
}
```

Ereignisspezifische Felder:

| `event_type` | Erforderliche Felder |
| --- | --- |
| `command_started` | `command_id: String`, `entity_ids: PackedStringArray` |
| `command_completed` | `command_id: String`, `entity_ids: PackedStringArray` |
| `command_failed` | `command_id: String`, `entity_ids: PackedStringArray`, `reason: StringName` |
| `entity_created` | `entity: Dictionary` |
| `entity_destroyed` | `entity_id: String`; optional: `owner_player_id`, `destroyed_by_entity_id` |
| `construction_completed` | `building_entity_id: String`, `builder_entity_id: String` |
| `production_completed` | `producer_entity_id: String`, `produced_entity_id: String`, `queue_item_id: String` |
| `research_completed` | `player_id: String`, `research_id: String` |
| `doctrine_chosen` | `player_id: String`, `doctrine_id: String` |
| `ability_activated` | `ability_id: String`, `caster_entity_ids: PackedStringArray`; optionales Ziel |
| `projectile_created` | `projectile_entity_id`, `source_entity_id`, `target_position`; optional: `target_entity_id` |
| `damage_applied` | `target_entity_id: String`, `amount: int`, `remaining_health: int`; optionale Quelle |
| `veterancy_gained` | `unit_entity_id: String`, `new_level: int` |
| `capital_level_gained` | `unit_entity_id: String`, `new_level: int`, `available_upgrade_ids: PackedStringArray` |
| `outpost_captured` | `outpost_entity_id: String`, `new_owner_player_id: String`; optionaler vorheriger Besitzer |
| `outpost_specialized` | `outpost_entity_id: String`, `specialization: OutpostSpecialization` |
| `resource_delivered` | `harvester_entity_id`, `refinery_entity_id`, `player_id`, `materials: int` |
| `resource_field_depleted` | `resource_field_entity_id: String` |
| `power_state_changed` | `player_id: String`, `is_low_power: bool`, `energy_produced: int`, `energy_required: int` |
| `player_under_attack` | `player_id: String`, `approximate_position: Vector2` |
| `player_defeated` | `player_id: String` |
| `destination_unreachable` | `command_id`, `unit_entity_ids`, `requested_destination`; optionale nächste Position |

Beispiel:

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

## Allgemeiner Fehler

Wird für Protokoll-, Autorisierungs-, Lobby- und Serverfehler verwendet, bei
denen es sich nicht um die Ablehnung eines Gameplay-Befehls handelt.

```gdscript
{
	"code": &"match_not_found", # StringName
	"retryable": false,         # bool

	# Optional:
	"details": "",              # String
}
```

Gültige Codes:

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

## Validierung

Die Interface-Bibliothek prüft die Implementierungsstruktur, während eine
Message Factory nicht vertrauenswürdige Netzwerkdaten prüft. Beide Prüfungen
sind erforderlich.

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
	if not Interfaces.implements(message, &"MessageInterface"):
		return null
	if not Interfaces.implements(message, &"CommandInterface"):
		return null
	if not Interfaces.implements(message, &"QueueableCommandInterface"):
		return null

	message.from_payload(payload)
	if not message.validate().is_empty():
		return null

	return message
```

Die Validierung muss außerdem die Gameplay-Regeln prüfen:

- Die Partie läuft.
- Der Absender ist ein aktiver, noch nicht besiegter Spieler.
- Der Absender besitzt jede befehligte Entität.
- Alle referenzierten Entitäten existieren und sind am Leben.
- Ziele sind gültig und, falls erforderlich, sichtbar.
- Die Einheit oder das Gebäude unterstützt den Befehl.
- Ressourcen, Energie, Flottenkapazität und Voraussetzungen sind verfügbar.
- Platzierung, Warteschlangengröße und Forschungsbeschränkungen sind erfüllt.

## Regeln zur Befehlsverarbeitung

1. Der Absender erstellt eine eindeutige `command_id`.
2. Der Mehrspielercode verpackt die Payload in einen eindeutigen Nachrichtenumschlag.
3. Die Message Factory validiert das rohe `Dictionary` und erstellt eine typisierte
   Interface-Implementierung.
4. Der Command Handler authentifiziert den Absender und validiert die Gameplay-Regeln.
5. Der Server sendet `COMMAND_RESULT`.
6. Angenommene Befehle gelangen in die passende Einheiten-, Gebäude- oder Forschungswarteschlange.
7. Der Simulation Core verarbeitet Befehle in festen Ticks.
8. Zustandsänderungen werden über `STATE_DELTA` verteilt.
9. Wichtige Ergebnisse werden über `SIMULATION_EVENTS` verteilt.

Doppelte `message_id`-Werte müssen ignoriert werden. Bei Wiederholung einer
bereits verarbeiteten `command_id` muss das ursprüngliche Ergebnis zurückgegeben
werden, ohne den Befehl erneut anzuwenden.

## Lokale Verwendung und KI

Menschliche Eingaben, KI und Mehrspieler übermitteln dieselben durch Interfaces
abgesicherten Nachrichtenobjekte:

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

Dadurch verwenden Einzelspieler, KI, Mehrspieler, Tutorials und automatisierte
Tests denselben maßgeblichen Befehlspfad.
