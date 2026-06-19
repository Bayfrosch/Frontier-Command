using Godot;
using System;

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
    public bool GettingRepaired { get; protected set; } = true;
    public int BuildProgression { get; protected set; } = 0;

    public virtual void ApplyState(BuildingState state)
    {
        EntityId = state.EntityId;
        OwnerPlayerId = state.OwnerPlayerId;
        Type = state.Type;
        Position = state.CurrentPosition;
        Health = state.Health;
        MaxHealth = state.MaxHealth;
        BuildProgression = state.BuildProgression;
    }
}
