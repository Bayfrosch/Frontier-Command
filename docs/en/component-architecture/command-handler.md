# Frontier Command - Command Handler Architecture

## Overview

The Command Handler is responsible for receiving, validating and storing game commands.

Commands represent player or AI intent.

The Command Handler is a central system because it connects the Client, AI, Multiplayer and Simulation Core.

---

# Purpose

The Command Handler ensures that all actions enter the game through the same controlled process later avoiding unnecessary network traffic.

Commands may come from:

* Human player input
* AI decision making
* Multiplayer network messages
* Debug tools

All of these sources should use the same command format.

---

# Command Philosophy

Commands describe intent.

They should not directly manipulate visual objects.

Examples:

* Move selected units to position
* Attack target
* Build structure
* Train unit
* Start research
* Capture Outpost
* Set rally point

The Simulation Core decides whether the command is valid and how it is executed.

---

# Command Flow

A typical command follows this process:

1. Command is created.
2. Command is sent to the Command Handler.
3. Command Handler validates the command.
4. Command is accepted or rejected.
5. Accepted command is added to the correct queue.
6. Simulation Core executes the queue.
7. Client displays the result.

---

# Command Queues

Frontier Command supports command queueing.

This allows players to assign multiple future actions to units, buildings or research.

---

# Unit Command Queue

Units may store a sequence of orders.

Example:

1. Move to location A.
2. Move to location B.
3. Attack enemy structure.
4. Capture Outpost.

The unit executes the first command in the queue.

When completed, the next command begins automatically.

---

# Building Command Queue

Buildings may also store queues.

Examples:

* Train unit A.
* Train unit B.
* Train unit C.
* Start upgrade.

Production structures use queues to manage unit creation and upgrades.

---

# Queue Modes

## Replace Queue

Default behavior.

The new command replaces the current queue.

Example:

* Player right-clicks a location.
* Selected units abandon previous orders.
* Selected units move to the new location.

---

## Add To Queue

Used when the player holds the queue modifier key.

The new command is added to the end of the existing queue.

Example:

* Player holds Shift.
* Player gives multiple movement orders.
* Units follow the route in sequence.

---

# Command Validation

Every command should be validated before execution.

Validation checks may include:

* Does the player own the unit?
* Is the target valid?
* Is the command allowed for this unit?
* Are required resources available?
* Is the structure placement valid?
* Is the research unlocked?
* Is the unit alive?
* Is the command still possible?

Invalid commands should be rejected cleanly.

---

# Command Completion

A command is completed when its objective is fulfilled.

Examples:

* Move command completes when the unit reaches the destination.
* Attack command completes when the target is destroyed or unreachable.
* Build command completes when construction finishes.
* Research command completes when the technology is unlocked.
* Capture command completes when the Outpost changes ownership.

After completion, the next command in the queue begins.

---

# Command Cancellation

Commands may be cancelled by:

* Player input
* Invalid target
* Unit death
* Building destruction
* Resource loss
* Ownership change

Cancelled commands should not break the queue.

The system should either continue with the next command or clear the queue depending on command type.

---

# Multiplayer Considerations

The Command Handler should be designed with multiplayer in mind from the beginning.

In multiplayer, clients should send commands rather than full unit state.

The server validates commands and distributes accepted commands.

This supports:

* Lower bandwidth
* Cleaner replay generation
* Better cheat prevention
* Consistent game logic