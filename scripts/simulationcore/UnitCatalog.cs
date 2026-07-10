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

    public static float GetMovementSpeed(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 30f,
            UnitType.CONSTRUCTION_UNIT => 30f,
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