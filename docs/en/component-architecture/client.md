# Frontier Command - Client Architecture

## Overview

The Client is responsible for presenting the game to the player and translating player actions into game commands.

The Client should not contain the authoritative game rules. Its main purpose is to handle rendering, input, camera movement, user interface, audio, and player feedback.

The actual game state should be managed by the Simulation Core.

---

# Responsibilities

The Client is responsible for:

* Rendering the battlefield
* Displaying units, buildings, effects, and UI
* Handling player input
* Managing the camera
* Showing selection and command feedback
* Playing sounds and music
* Displaying game information
* Creating commands from player actions

The Client should not be responsible for:

* Deciding combat results
* Managing resource rules
* Resolving research effects
* Determining win or loss
* Running authoritative multiplayer logic

---

# Main Client Systems

## Rendering System

Displays the current game state.

Responsibilities:

* Draw terrain
* Draw units
* Draw buildings
* Draw projectiles
* Draw effects
* Draw selection indicators
* Draw fog of war

The rendering system should read from the game state but should not change it directly.

---

## Input System

Converts player actions into intent.

Examples:

* Select units
* Move camera
* Issue move command
* Issue attack command
* Place building
* Start research
* Train unit

Input should produce game commands rather than directly modifying game objects.

---

## Camera System

Controls battlefield navigation.

Responsibilities:

* Move camera
* Zoom camera
* Keep camera within map bounds

For Frontier Command, the camera should be fixed-angle.

Camera rotation is not required.

---

## Selection System

Handles unit and building selection.

Responsibilities:

* Single selection
* Box selection
* Group selection
* Selection filtering
* Display selected unit information

Selection should be client-side because it is only relevant to the local player.

---

## Command Feedback System

Provides visual confirmation of player orders.

Examples:

* Move command marker
* Attack command marker
* Building placement preview
* Invalid placement warning
* Rally point indicator

Good feedback is critical for responsive RTS controls.

---

## User Interface System

Displays player information and controls.

Responsibilities:

* Resources
* Energy
* Unit production
* Building production
* Research options
* Selected unit panel
* Minimap
* Alerts
* Victory/defeat screen

The UI should show information clearly without overwhelming the player.

---

## Audio System

Provides sound feedback.

Responsibilities:

* Unit acknowledgements
* Combat sounds
* Construction sounds
* Warning alerts
* Music
* UI sounds

Audio should help the player understand what is happening without constantly looking at every part of the map.

---

# Client and Simulation Separation

The Client should communicate with the Simulation Core through commands.

Example flow:

1. Player selects units.
2. Player right-clicks a location.
3. Client creates a Move Command.
4. Simulation Core validates the command.
5. Simulation Core updates unit orders.
6. Client renders the result.

This separation is important because the same command system can later support:

* AI players
* Multiplayer
* Replays
* Tutorials
* Automated testing

---

# Command Examples

Common player commands include:

* Move units
* Attack target
* Stop units
* Patrol area
* Build structure
* Repair structure
* Train unit
* Start research
* Capture Outpost
* Set rally point

Commands should describe player intent, not visual effects.

---

# Client-Side State

Some information exists only on the Client.

Examples:

* Current camera position
* Current selection
* UI panel state
* Mouse position
* Local control groups
* Local hotkeys
* Visual previews

This state does not need to be synchronized in multiplayer.

---