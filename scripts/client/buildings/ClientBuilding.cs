using Godot;
using System;
using System.Collections.Generic;

/*
Building object used in client to render entities
Cannot change actual values can only display them
*/
public abstract partial class ClientBuilding : Node2D, ClientEntity
{
    public string EntityId { get; protected set; } = "";
    public BuildingType Type { get; protected set; }
    public string? OwnerPlayerId { get; protected set; }
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public HashSet<Ability> Abilities { get; protected set; }
    public bool GettingRepaired { get; protected set; } = true;
    public int BuildProgression { get; protected set; } = 0;
    
    /*
    Called once on spawning Building
    */
    public virtual void ApplySpawnState(BuildingState state)
    {
        ApplyState(state);
        Position = state.CurrentPosition;
        Abilities = state.Abilities;
    }

    /*
    Called every Tick
    */
    public virtual void ApplyState(BuildingState state)
    {
        EntityId = state.EntityId;
        OwnerPlayerId = state.OwnerPlayerId;
        Type = state.Type;
        Health = state.Health;
        MaxHealth = state.MaxHealth;
        BuildProgression = state.BuildProgression;
    }

    protected static void ApplyProductionProgress(ProgressBar productionProgressBar, BuildingState state)
    {
        if (state.ProductionQueue.Length <= 0)
        {
            productionProgressBar.Visible = false;
            productionProgressBar.Value = 0;
            productionProgressBar.MaxValue = 1;
            return;
        }

        productionProgressBar.MaxValue = UnitCatalog.GetProductionTime(state.ProductionQueue[0]);
        productionProgressBar.Value = state.ProductionProgress;
        productionProgressBar.Visible = true;
    }
}
