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
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    public static int GetAttackDamage(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 10,
            UnitType.CONSTRUCTION_UNIT => 0,
            UnitType.OVERLORD => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static float GetAttackRange(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 120f,
            UnitType.CONSTRUCTION_UNIT => 0f,
            UnitType.OVERLORD => 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static float GetAttackWindupTime(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 0.4f,
            UnitType.CONSTRUCTION_UNIT => 0f,
            UnitType.OVERLORD => 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static int GetMaxHealth(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100,
            UnitType.CONSTRUCTION_UNIT => 150,
            UnitType.OVERLORD => 1000,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static float GetMovementSpeed(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 100f,
            UnitType.CONSTRUCTION_UNIT => 100f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }

    public static List<UnitType> GetCapitalUnits()
    {
        return new List<UnitType>
        {
            UnitType.OVERLORD,
        };
    }
}
