# Frontier Command - Developer Handbook

## Purpose

This handbook is a practical guide for working on the current Frontier Command prototype.

It focuses on where gameplay concepts live in the codebase and how to extend them safely. It does not cover project setup, dependency installation, release packaging, deployment, or general Godot/C# build instructions.

Use this document when adding:

* Units
* Buildings
* Abilities
* Commands
* Client scenes
* Simulation rules

---

# Current Architecture

## Simulation Core

The Simulation Core owns the authoritative game state and gameplay rules.

Important files:

* `scripts/simulationcore/SimulationContext.cs`
* `scripts/simulationcore/MatchState.cs`
* `scripts/simulationcore/UnitCatalog.cs`
* `scripts/simulationcore/BuildingCatalog.cs`
* `scripts/simulationcore/catalogues/WeaponEffectivenessCatalog.cs`
* `scripts/simulationcore/AbilityCatalog.cs`

The Simulation Core decides things like:

* Entity creation and removal
* Unit movement
* Attack behavior
* Construction progress
* Production progress
* Health and damage
* Ability effects

The Client should not decide authoritative gameplay results.

## Client

The Client renders state and turns player input into commands.

Important files:

* `scripts/client/GameScene.cs`
* `scripts/client/ClientWorldRenderer.cs`
* `scripts/client/SelectedEntityUI.cs`
* `scripts/client/LocalSimulationNode.cs`
* `scripts/client/units/ClientUnit.cs`
* `scripts/client/buildings/ClientBuilding.cs`

The Client decides things like:

* Selection
* Mouse input
* UI buttons
* Construction preview
* Rendering units and buildings
* Sending commands to the local simulation

---

# Important Current Concepts

## Entities

All game objects derive from `EntityState` in `MatchState.cs`.

Current entity types:

* `UnitState`
* `BuildingState`
* `OutpostState`
* `ResourceState`

Shared entity data includes:

* `EntityId`
* `OwnerPlayerId`
* `CurrentPosition`
* `Health`
* `MaxHealth`
* `Abilities`

## Units

Unit types are defined by `UnitType` in `MatchState.cs`.

Current unit types:

* `BASIC_INFANTRY`
* `RPG_TROOPER`
* `CONSTRUCTION_UNIT`

Unit gameplay values live in `UnitCatalog.cs`.

Current unit features include:

* Movement
* Formation movement
* Attack orders
* Attack wind-up
* Construction orders for construction units

## Buildings

Building types are defined by `BuildingType` in `MatchState.cs`.

Current building types:

* `CONSTRUCTION_SITE`
* `BARRACKS`
* `RESOURCE_SPAWNER`

Building gameplay values live in `BuildingCatalog.cs`.

Buildings may:

* Be under construction
* Complete into another building type
* Have abilities
* Produce units
* Spawn and track resource sources
* Be sold if completed
* Be cancelled if still a construction site

Resource spawners create neutral `ResourceState` entries around themselves when they complete construction. Each `ResourceState` stores its resource type, max amount, current amount, and source spawner id. The completed spawner tracks the ids it created in `ResourceEntityIds`.

## Abilities

Abilities are defined in `AbilityCatalog.cs`.

An ability has:

* `Id`
* `Name`
* `Unlocked`
* `Cost`
* `RequiresTarget`

The UI reads abilities from the selected entity and creates buttons automatically.

When a button is pressed, `SelectedEntityUI` emits `AbilityPressed`, and `GameScene` sends a `UseAbilityMessage`.

The actual effect is handled in `SimulationContext.HandleUseAbility`.

---

# Simulation Tick Flow

The current tick flow in `SimulationContext.AdvanceTick()` roughly does this:

1. Increment the match tick.
2. Advance projectiles.
3. Iterate players and entities.
4. Advance construction for buildings.
5. Advance production for buildings.
6. Advance unit attacks.
7. Advance unit movement.
8. Collect destroyed entities.
9. Remove destroyed entities and projectiles.
10. Spawn produced units.

When adding behavior, prefer adding it to the tick flow if it is ongoing gameplay logic.

Examples:

* Attack range checks belong in the tick system.
* Construction progress belongs in the tick system.
* Projectile movement belongs in the tick system.

Command handlers should usually assign intent, not repeatedly perform ongoing behavior.

---

# Adding a New Unit

This is the current checklist for adding a new unit.

## 1. Add the Unit Type

Edit `scripts/simulationcore/MatchState.cs`.

Add the unit to `UnitType`:

```csharp
public enum UnitType
{
	BASIC_INFANTRY,
	CONSTRUCTION_UNIT,
	OVERLORD,
	NEW_UNIT,
}
```

## 2. Add Unit Catalog Values

Edit `scripts/simulationcore/UnitCatalog.cs`.

Add the new unit to every relevant method:

* `GetProductionTime`
* `GetAttackDamage`
* `GetWeaponClass`
* `GetArmorClass`
* `GetAttackRange`
* `GetAttackWindupTime`
* `GetAttackCooldownTime`
* `GetMaxHealth`
* `GetMovementSpeed`
* `RequiresEnergyForProduction`, if production should pause during a power deficit
* `GetCapitalUnits`, only if it is a capital unit

If the unit cannot attack, use:

```csharp
UnitType.NEW_UNIT => 0
```

for attack damage/range/wind-up/cooldown where appropriate.

Do not leave the unit out of a switch unless you intentionally want that unit to throw at runtime.

Energy-dependent units remain in their building queue at their current progress while
`PlayerState.EnergyProduced < PlayerState.EnergyConsumed`. Production resumes automatically
when the player's power balance is no longer negative.

## 3. Add Unit Abilities

Edit `scripts/simulationcore/AbilityCatalog.cs`.

Add abilities in `ForUnit(UnitType type)`:

```csharp
case UnitType.NEW_UNIT:
	abilities.Add(new Ability
	{
		Id = "some_ability_id",
		Name = "Visible Name",
		Unlocked = true,
		Cost = 0,
		RequiresTarget = false,
	});
	break;
```

If the ability requires a target position, set `RequiresTarget = true`.
Do not add the ability to a separate target-requirement switch; the `Ability` object is the source of truth.

## 4. Add a Client Scene

Create a scene under:

* `scenes/units`

Current examples:

* `scenes/units/basicInfantry.tscn`
* `scenes/units/constructionUnit.tscn`

The scene should have a script derived from `ClientUnit`.

Current scripts live under:

* `scripts/client/units`

If the unit does not need custom rendering behavior, a small subclass like `BasicInfantry.cs` or `ConstructionUnit.cs` is enough.

The scene should expose the same important child nodes expected by `ClientUnit`:

* `BodyRender`
* `HealthBar`

## 5. Register the Scene in the Renderer

Edit `scripts/client/ClientWorldRenderer.cs`.

Add a `PackedScene` field:

```csharp
public PackedScene NewUnitScene { get; } = GD.Load<PackedScene>("res://scenes/units/newUnit.tscn");
```

Add it to `GetUnitScene`:

```csharp
UnitType.NEW_UNIT => NewUnitScene,
```

Optionally add a color in `GetDefaultUnitColor`.

## 6. Make the Unit Producible

If a building should produce this unit, edit `AbilityCatalog.ForBuilding`.

Example:

```csharp
abilities.Add(new Ability
{
	Id = "spawn_new_unit",
	Name = "New Unit",
	Unlocked = true,
	Cost = 20,
	RequiresTarget = false
});
```

Then edit `SimulationContext.HandleUseAbility`.

Add a case:

```csharp
case "spawn_new_unit":
	passed = HandleSpawnUnit(msg, UnitType.NEW_UNIT);
	break;
```

## 7. Add Tests

Add focused tests in:

* `__tests__/simulationcore/SimulationCoreTest.cs`

Useful tests:

* Unit can be spawned or produced.
* Unit has expected health/speed/damage.
* Unit ability produces the intended command/effect.
* Unit can render if a scene mapping was added.

---

# Adding a New Building

This is the current checklist for adding a new building.

## 1. Add the Building Type

Edit `scripts/simulationcore/MatchState.cs`.

Add the building to `BuildingType`:

```csharp
public enum BuildingType
{
	CONSTRUCTION_SITE,
	BARRACKS,
	NEW_BUILDING,
}
```

## 2. Add Building Catalog Values

Edit `scripts/simulationcore/BuildingCatalog.cs`.

Add the building to:

* `GetFoodprintSize`
* `GetMaxHealth`
* `GetPowerProduction`, only if the building provides Energy
* `GetPowerConsumption`, only if the building drains Energy

Example:

```csharp
BuildingType.NEW_BUILDING => new Vector2(100, 80)
```

and:

```csharp
BuildingType.NEW_BUILDING => 700
```

Energy-producing buildings should return their produced capacity from `GetPowerProduction`:

```csharp
BuildingType.NEW_BUILDING => 10
```

Buildings that do not provide Energy should use the default `0`.

Energy-consuming buildings should return their drained amount from `GetPowerConsumption`:

```csharp
BuildingType.NEW_BUILDING => 2
```

Buildings that do not consume Energy should use the default `0`.

`GetFootprintType` usually does not need changes unless construction-site behavior changes.

## 3. Add Building Abilities

Edit `scripts/simulationcore/AbilityCatalog.cs`.

Add only building-specific completed-building abilities in `ForBuilding`.

Example:

```csharp
case BuildingType.NEW_BUILDING:
	abilities.Add(new Ability
	{
		Id = "spawn_new_unit",
		Name = "New Unit",
		UnlockedFromStart = true,
		Cost = 100,
		RequiresTarget = false,
	});
	break;
```

Set `RequiresTarget` on each building ability. Completed-building actions like selling or producing units usually use `false`; abilities that ask the player to choose a map position use `true`.

Construction sites currently get:

* `cancel_construction`

Completed buildings automatically get:

* `sell_building`

Do not add `sell_building` manually to each completed building case. `AbilityCatalog.ForBuilding` adds it once for every building type except `CONSTRUCTION_SITE`.

For construction abilities with building prerequisites, set `UnlockedFromStart = false` and
update `PlayerState.UpdateAbilityUnlocks`. For example, `spawn_war_factory` is unlocked while
the player has at least one completed `RESOURCE_GATHERER` (Supply Center).

Completed buildings can also get:

* production abilities
* future research/upgrade abilities

## 4. Add Build Ability to a Unit

If construction units should build the new building, edit `AbilityCatalog.ForUnit`.

For `UnitType.CONSTRUCTION_UNIT`, add:

```csharp
abilities.Add(new Ability
{
	Id = "spawn_new_building",
	Name = "New Building",
	Unlocked = true,
	Cost = 1000,
	RequiresTarget = true,
});
```

Build abilities should set `RequiresTarget = true` because `GameScene` uses that field from the selected ability to enter target selection before sending `UseAbilityMessage`.

## 5. Route the Ability in SimulationContext

Edit `SimulationContext.HandleUseAbility`.

Add:

```csharp
case "spawn_new_building":
	passed = HandleSpawnBuilding(msg, BuildingType.NEW_BUILDING);
	break;
```

`HandleSpawnBuilding` creates a `BuildStructureMessage`.

`HandleBuildStructure` creates a `BuildingState` as a `CONSTRUCTION_SITE` with `pendingBuilding` set to the requested completed building type.

When construction reaches 100%, `BuildingState.AdvanceConstruction` changes the building type and loads completed-building abilities.

## 6. Add a Client Scene

Create a scene under:

* `scenes/buildings`

Current examples:

* `scenes/buildings/ConstructionSite.tscn`
* `scenes/buildings/Barracks.tscn`
* `scenes/buildings/PowerPlant.tscn`
* `scenes/buildings/WarFactory.tscn`

The scene should have a script derived from `ClientBuilding`.

Current scripts live under:

* `scripts/client/buildings`

Use the existing client building scripts when possible:

* `SimpleBuilding` for buildings with no special client-side UI.
* `ProductionBuilding` for buildings with `BodyRender/ProductionProgressBar`.
* `ConstructionSite` for construction-site rendering.
* A custom `ClientBuilding` subclass only when the scene needs unique client rendering behavior.

If the building has production or construction UI, add the relevant progress bar and update it in the client script. Production buildings should expose:

* `BodyRender`
* `BodyRender/ProductionProgressBar`

Every selectable building scene should expose:

* `Area2D`
* `Area2D/CollisionShape2D`

## 7. Register the Scene in the Renderer

Edit `scripts/client/ClientWorldRenderer.cs`.

Add a `PackedScene` field:

```csharp
public PackedScene NewBuildingScene { get; } = GD.Load<PackedScene>("res://scenes/buildings/NewBuilding.tscn");
```

Add it to `GetBuildingScene`:

```csharp
BuildingType.NEW_BUILDING => NewBuildingScene,
```

If the building starts as a construction site, the renderer will initially render `ConstructionSiteScene`. When construction completes, `SyncBuilding` replaces the scene if the type changed.

## 8. Add Construction Preview Support

Construction preview is currently handled in `GameScene.cs`.

For a new build ability:

* Set `RequiresTarget = true` on the ability in `AbilityCatalog`.
* Add the ability id to `GameScene.ConstructionPreviewTypes`.
* Map the ability id to the completed `BuildingType`.
* Make sure `BuildingCatalog.GetFoodprintSize` has the correct footprint.

Example:

```csharp
["spawn_new_building"] = BuildingType.NEW_BUILDING
```

The preview uses `ConstructionSite.tscn` and resizes its body/collision footprint from `BuildingCatalog`.

## 9. Add Tests

Add focused tests in:

* `__tests__/simulationcore/SimulationCoreTest.cs`

Useful tests:

* Build ability creates a construction site.
* Construction site has the correct pending building type.
* Assigned construction unit builds only that site.
* Completed building receives completed-building abilities.
* Sell removes completed building.
* Completed buildings automatically receive `sell_building`; construction sites do not.
* Energy-producing buildings update `PlayerState.EnergyProduced`.
* Energy-consuming buildings update `PlayerState.EnergyConsumed`.
* Cancel removes construction site.

---

# Adding a New Ability

Abilities have three parts:

1. Catalog definition
2. Client button display
3. Simulation effect

## 1. Add the Ability to the Catalog

Edit `scripts/simulationcore/AbilityCatalog.cs`.

Add the ability to either:

* `ForUnit`
* `ForBuilding`

Example:

```csharp
abilities.Add(new Ability
{
	Id = "ability_id",
	Name = "Button Text",
	Unlocked = true,
	Cost = 0,
	RequiresTarget = false
});
```

## 2. Define Whether It Needs a Target

Still in `AbilityCatalog.cs`, set `RequiresTarget` on the `Ability` object.

Examples:

```csharp
RequiresTarget = false,
RequiresTarget = true,
```

If this is omitted, the default is `false`; targeted abilities should set it explicitly to `true`.

## 3. Implement the Effect

Edit `SimulationContext.HandleUseAbility`.

Add a case:

```csharp
case "ability_id":
	passed = HandleSomeAbility(msg);
	break;
```

For larger behavior, create a helper method near the other handlers.

Current ability examples:

* `spawn_infantry`
* `spawn_barracks`
* `cancel_construction`
* `sell_building`

## 4. Add Client Feedback if Needed

Targeted abilities use `GameScene` target selection.

If the ability needs special preview or cursor feedback, add it in:

* `GameScene.OnAbilityPressed`
* `GameScene.ShowConstructionPreview`
* `GameScene.ClearAbilityTargetSelection`

Do not implement authoritative gameplay effects in the client.

## 5. Add Tests

Add tests for:

* Ability is accepted.
* Ability rejects invalid caster/target.
* Ability changes simulation state correctly.

---

# Adding a New Command

Use a new command when player intent does not fit an existing message.

## 1. Add Message Class

Create a file under:

* `scripts/messages/commands`

Follow existing command patterns.

The command should:

* Derive from `MessageBase`
* Implement `CommandInterface`
* Implement `QueueableCommandInterface` if queue behavior matters
* Implement `to_payload`
* Implement `from_payload`
* Implement `validate`

## 2. Add Message Type Constant

Edit:

* `scripts/messages/GameMessages.cs`

Add a `StringName` constant for the message type.

## 3. Register Handler

Edit `SimulationContext` constructor.

Add:

```csharp
Register<NewCommandMessage>(HandleNewCommand);
```

Then implement:

```csharp
private bool HandleNewCommand(NewCommandMessage msg)
{
	...
}
```

## 4. Send It From Client

Edit `GameScene.cs` or the relevant UI script.

The Client should create the command and call:

```csharp
simulationCore.Push(command);
```
