# Frontier Command - AI System

## Overview

The AI System is responsible for controlling non-human players.

AI opponents should follow the same rules as human players and interact with the game exclusively through the Command Handler.

The AI should not receive hidden advantages or bypass normal game mechanics.

Its purpose is to provide challenging and believable opponents while also serving as a testing tool during development.

---

# Design Philosophy

The AI should behave like a commander rather than a script.

It should:

* Expand territory
* Gather resources
* Build infrastructure
* Research technologies
* Assemble armies
* Attack opponents
* Defend important positions

The AI should pursue strategic goals while adapting to changing battlefield conditions.

---

# Responsibilities

The AI is responsible for:

* Economic management
* Base construction
* Expansion
* Outpost capture
* Research decisions
* Army production
* Army composition
* Scouting
* Defense
* Offensive operations

---

# Economic Management

The AI should:

* Build harvesters
* Expand resource gathering
* Maintain energy production
* Balance spending priorities

The AI should continuously decide how resources are allocated between:

* Economy
* Research
* Military production

---

# Expansion

The AI should recognize valuable expansion opportunities.

Examples:

* Nearby Outposts
* Resource-rich regions

Expansion should be a major part of its strategy.

---

# Research

The AI should:

* Invest in technology
* Select doctrines
* Adapt research priorities

Different AI personalities may favor different doctrine paths.

---

# Military Production

The AI should maintain an effective military force.

Responsibilities include:

* Producing units
* Replacing losses
* Responding to enemy threats
* Building balanced armies

The AI should avoid relying on a single unit type whenever possible.

---

# Defense

The AI should protect:

* Main bases
* Resource operations
* Outposts
* Strategic locations

The AI should react to attacks and reinforce threatened areas when possible.

---

# Offense

The AI should launch attacks when opportunities exist.

Examples:

* Weakly defended expansions
* Isolated Outposts
* Enemy economic targets
* Major offensives against strategic positions

The AI should not attack continuously without purpose.

---

# Difficulty Levels

Difficulty should primarily affect decision quality.

Preferred adjustments:

* Better expansion decisions
* Faster reactions
* Improved army composition
* Stronger strategic planning

Avoid excessive economic cheating whenever possible.

Small bonuses may be acceptable on higher difficulties.

---

# AI Personalities

Future AI personalities may emphasize different playstyles.

Examples:

### Aggressive

* Expands quickly
* Attacks frequently
* Prioritizes military production

---

### Defensive

* Focuses on fortification
* Expands cautiously
* Prefers stronger economies