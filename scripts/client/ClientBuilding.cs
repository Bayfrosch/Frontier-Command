using Godot;
using System;

public abstract partial class ClientBuilding : Node2D, ClientEntity
{
    public const int REPAIR_RATE = 10;
    public string EntityId { get; protected set; } = "";
    public BuildingType Type { get; protected set; }
    public string? OwnerPlayerId { get; protected set; }
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public bool GettingRepaired { get; protected set; } = true;
    public bool InConstruction { get; protected set; } = true;

    public virtual void ApplyDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
    }

    public virtual void ApplyState(BuildingState state)
    {
        EntityId = state.EntityId;
        OwnerPlayerId = state.OwnerPlayerId;
        Type = state.Type;
        Position = state.CurrentPosition;
        Health = state.Health;
        MaxHealth = state.MaxHealth;
    }
}
