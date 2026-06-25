using System;
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

    public static float GetMovementSpeed(UnitType type)
    {
        return type switch
        {
            UnitType.BASIC_INFANTRY => 15f,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)  
        };
    }
}