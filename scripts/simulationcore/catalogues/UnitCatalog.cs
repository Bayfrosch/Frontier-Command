using System;
using System.Collections.Generic;
using Godot;

public static class UnitCatalog
{
    public static int GetProductionTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100,
            UnitType.RPG_TROOPER => 120,
            UnitType.CONSTRUCTION_UNIT => 100,
            UnitType.RESOURCE_COLLECTOR => 150,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    public static int GetAttackDamage(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 10,
            UnitType.RPG_TROOPER => 30,
            _ => 0 // All not listed are non combat units (0 damage)
        };
    }

    public static WeaponClass GetWeaponClass(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => WeaponClass.SMALL_ARMS,
            UnitType.RPG_TROOPER => WeaponClass.ANTI_ARMOR,
            UnitType.CONSTRUCTION_UNIT => WeaponClass.NON_COMBAT,
            UnitType.RESOURCE_COLLECTOR => WeaponClass.NON_COMBAT,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static ArmorClass GetArmorClass(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => ArmorClass.LIGHT,
            UnitType.RPG_TROOPER => ArmorClass.LIGHT,
            UnitType.CONSTRUCTION_UNIT => ArmorClass.MEDIUM,
            UnitType.RESOURCE_COLLECTOR => ArmorClass.MEDIUM,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static float GetAttackRange(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 120f,
            UnitType.RPG_TROOPER => 120f,
            _ => 0f // All not listed are non combat units (0 range)
        };
    }

    public static float GetAttackWindupTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 0.4f,
            UnitType.RPG_TROOPER => 1f,
            _ => 0f // All not listed are non combat units (0 wind up)
        };
    }

    public static float GetAttackCooldownTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 0.4f,
            UnitType.RPG_TROOPER => 1.5f,
            _ => 0f // All not listed are non combat units (0 cooldown) 
        };
    }

    public static int GetMaxHealth(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100,
            UnitType.RPG_TROOPER => 100,
            UnitType.CONSTRUCTION_UNIT => 250,
            UnitType.RESOURCE_COLLECTOR => 150,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static float GetMovementSpeed(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100f,
            UnitType.RPG_TROOPER => 80f,
            UnitType.CONSTRUCTION_UNIT => 100f,
            UnitType.RESOURCE_COLLECTOR => 100f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    public static List<UnitType> GetCapitalUnits()
    {
        return new List<UnitType>
        {
        };
    }
}
