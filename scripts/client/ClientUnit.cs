using Godot;
using System;

public abstract partial class ClientUnit : Node2D, ClientEntity
{
    public string EntityId { get; protected set; }
    public string? OwnerPlayerId { get; protected set; }
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public bool GettingRepaired { get; protected set; }
    public UnitType Type { get; protected set; }
    public Vector2 TargetPosition { get; protected set; }
    public float MovementSpeed { get; protected set; }

    public virtual void ApplyState(UnitState state)
    {
        EntityId = state.EntityId;
        OwnerPlayerId = state.OwnerPlayerId;
        Health = state.Health;
        MaxHealth = state.MaxHealth;
        GettingRepaired = state.GettingRepaired;
        Type = state.Type;
        Position = state.CurrentPosition;
        TargetPosition = state.TargetPosition;
        MovementSpeed = state.MovementSpeed;
    }
}
