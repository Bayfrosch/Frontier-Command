using System;
using System.Collections.Generic;
using Godot;

public static class UnitCatalog
{
    /*
    Production time is advanced by BuildingState each simulation tick.
    */
    public static int GetProductionTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100,
            UnitType.RPG_TROOPER => 120,
            UnitType.LIGHT_TANK => 200,
            UnitType.CONSTRUCTION_UNIT => 100,
            UnitType.RESOURCE_COLLECTOR => 150,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    /*
    Returns base weapon damage.
    Non-combat units return 0 and cannot receive attack orders.
    */
    public static int GetAttackDamage(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 10,
            UnitType.RPG_TROOPER => 30,
            UnitType.LIGHT_TANK => 45,
            _ => 0 // All not listed are non combat units (0 damage)
        };
    }

    /*
    Weapon class is combined with target armor to calculate final damage.
    */
    public static WeaponClass GetWeaponClass(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => WeaponClass.SMALL_ARMS,
            UnitType.RPG_TROOPER => WeaponClass.ANTI_ARMOR,
            UnitType.LIGHT_TANK => WeaponClass.CANNON,
            UnitType.CONSTRUCTION_UNIT => WeaponClass.NON_COMBAT,
            UnitType.RESOURCE_COLLECTOR => WeaponClass.NON_COMBAT,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /*
    Armor class is used by WeaponEffectivenessCatalog for incoming damage.
    */
    public static ArmorClass GetArmorClass(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => ArmorClass.LIGHT,
            UnitType.RPG_TROOPER => ArmorClass.LIGHT,
            UnitType.LIGHT_TANK => ArmorClass.MEDIUM,
            UnitType.CONSTRUCTION_UNIT => ArmorClass.MEDIUM,
            UnitType.RESOURCE_COLLECTOR => ArmorClass.MEDIUM,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /*
    Attack range controls how close a unit must be before firing.
    */
    public static float GetAttackRange(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 120f,
            UnitType.RPG_TROOPER => 120f,
            UnitType.LIGHT_TANK => 250f,
            _ => 0f // All not listed are non combat units (0 range)
        };
    }

    /*
    Windup is the delay between reaching attack range and applying damage.
    */
    public static float GetAttackWindupTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 0.4f,
            UnitType.RPG_TROOPER => 1f,
            UnitType.LIGHT_TANK => 0.8f,
            _ => 0f // All not listed are non combat units (0 wind up)
        };
    }

    /*
    Cooldown is the delay after an attack before the unit can fire again.
    */
    public static float GetAttackCooldownTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 0.4f,
            UnitType.RPG_TROOPER => 1.5f,
            UnitType.LIGHT_TANK => 1.6f,
            _ => 0f // All not listed are non combat units (0 cooldown) 
        };
    }

    /*
    Max health is assigned when UnitState is created.
    */
    public static int GetMaxHealth(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100,
            UnitType.RPG_TROOPER => 100,
            UnitType.LIGHT_TANK => 450,
            UnitType.CONSTRUCTION_UNIT => 250,
            UnitType.RESOURCE_COLLECTOR => 150,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /*
    Movement speed is used by UnitState.AdvanceMovement.
    */
    public static float GetMovementSpeed(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100f,
            UnitType.RPG_TROOPER => 80f,
            UnitType.LIGHT_TANK => 70f,
            UnitType.CONSTRUCTION_UNIT => 100f,
            UnitType.RESOURCE_COLLECTOR => 100f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    /*
    Used by selection UI to pick the most important unit in a group.
    */
    public static List<UnitType> GetCapitalUnits()
    {
        return new List<UnitType>
        {
        };
    }

    /*
    Power-dependent units pause at their current production progress during a power deficit.
    */
    public static bool RequiresEnergyForProduction(UnitType type)
    {
        return type == UnitType.LIGHT_TANK;
    }
}
