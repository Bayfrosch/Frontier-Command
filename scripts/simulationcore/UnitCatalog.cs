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
