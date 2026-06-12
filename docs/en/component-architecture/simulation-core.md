# Frontier Command - Simulation Core Architecture

## Overview

The Simulation Core is the authoritative heart of Frontier Command.

It is responsible for maintaining and updating the actual game state. All gameplay rules, world logic, combat resolution, economy updates, research progress, unit behavior, territory control, and victory conditions are handled by the Simulation Core.

---

# Purpose

The Simulation Core exists to keep all gameplay logic centralized and consistent.

It should be usable by:

* Single-player matches
* Multiplayer servers
* AI matches

---

# Responsibilities

The Simulation Core is responsible for:

* Storing the current match state
* Updating units and buildings
* Resolving movement
* Resolving combat
* Managing resources
* Processing construction
* Processing production
* Processing research
* Managing neutral defenders
* Checking victory and defeat conditions
* Producing game events for the Client

---

# Simulation Tick

The Simulation Core advances the game in fixed simulation steps.

A fixed tick rate makes the game easier to test, synchronize, and eventually support in multiplayer.

Example tick flow:

1. Receive accepted commands.
2. Apply new orders.
3. Update movement.
4. Update combat.
5. Update construction.
6. Update production.
7. Update research.
8. Update economy.
9. Update territory and Outposts.
10. Check victory conditions.
11. Emit game events.

---

# Game State

The Simulation Core owns the authoritative game state.

This includes:

* Players
* Factions
* Resources
* Units
* Buildings
* Projectiles
* Research status
* Production queues
* Command queues
* Outposts
* Neutral defenders
* Fog of war
* Match timer
* Victory state

Other systems may read this state, but only the Simulation Core should modify it directly.

---

# Core Systems

## Unit System

Manages all unit-related logic.

Responsibilities:

* Unit creation
* Unit destruction
* Movement
* Pathing requests
* Command queue execution
* Health
* Status effects
* Unit ownership

---

## Building System

Manages structures.

Responsibilities:

* Building placement validation
* Construction progress
* Building ownership
* Building destruction
* Building upgrades
* Structure power usage
* Structure functionality

---

## Economy System

Manages player resources.

Responsibilities:

* Material collection
* Energy generation
* Resource spending
* Resource storage
* Economic bonuses
* Outpost economic effects

---

## Research System

Manages technological progression.

Responsibilities:

* Research availability
* Research costs
* Research progress
* Doctrine selection
* Technology unlocks
* Research effects

---

## Combat System

Manages combat resolution.

Responsibilities:

* Target validation
* Weapon cooldowns
* Damage calculation
* Armor interaction
* Projectile creation
* Area damage
* Unit destruction

Combat should be readable, predictable, and consistent.

---

## Movement System

Manages unit movement.

Responsibilities:

* Moving units toward targets
* Handling formations
* Avoiding collisions
* Responding to pathfinding results
* Supporting large armies

Movement must be efficient because Frontier Command targets medium-large battles with up to approximately 150 units per player in late game.

---

## Neutral System

Manages neutral defenders and uncontrolled Outposts.

Responsibilities:

* Neutral unit behavior
* Outpost defense groups
* Neutral respawn rules if applicable
* Neutral aggression rules

Neutral forces should limit free expansion and create early-game objectives.

---

## Fog of War System

Manages player vision.

Responsibilities:

* Visible areas
* Previously explored areas
* Hidden enemy movement
* Detection rules
* Scout value

Fog of war is critical for scouting, surprise attacks, and strategic uncertainty.

---

## Victory System

Manages match conclusion.

Responsibilities:

* Victory condition checks
* Defeat condition checks
* Player elimination
* Team victory
* End-of-match events

Victory conditions should be clear and predictable.

---

# Events

The Simulation Core should emit events when important things happen.

Examples:

* Unit created
* Unit destroyed
* Building completed
* Research completed
* Outpost captured
* Player attacked
* Resource depleted
* Victory achieved

The Client can use these events for:

* Visual effects
* Sound effects
* UI alerts
* Notifications

Events should describe what happened without depending on presentation details.

---

# Multiplayer Considerations

In multiplayer, the Simulation Core should run on the server.

Clients send commands to the server.

The server validates commands, updates the Simulation Core, and sends state updates or events back to clients.

Clients should not be authoritative over gameplay results.

This prevents many forms of cheating and keeps match logic consistent.

---

# Single-Player Considerations

In single-player, the Simulation Core runs locally in the same application as the Client.

The architecture should remain the same:

* Player input creates commands.
* AI creates commands.
* Command Handler validates commands.
* Simulation Core updates the game.

This ensures single-player and multiplayer use the same gameplay logic.
