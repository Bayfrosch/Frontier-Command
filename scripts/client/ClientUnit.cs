using Godot;
using System;
using System.Collections.Generic;

public abstract partial class ClientUnit : Node2D, ClientEntity
{
    public string EntityId { get; protected set; }
    public string? OwnerPlayerId { get; protected set; }
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public bool GettingRepaired { get; protected set; }
    public HashSet<Ability> Abilities { get; protected set; }
    public UnitType Type { get; protected set; }
    public Vector2 RenderPosition { get; protected set; }
    public Vector2 TargetPosition { get; protected set; }
    public float MovementSpeed { get; protected set; }


    /*
    Called on spawning Unit
    */
    public virtual void ApplySpawnState(UnitState state)
    {
        ApplyState(state);
        RenderPosition = state.CurrentPosition;
        Position = state.CurrentPosition;
        Abilities = state.Abilities;
    }
    /*
    Called on every Tick
    */
    public virtual void ApplyState(UnitState state)
    {
        EntityId = state.EntityId;
        OwnerPlayerId = state.OwnerPlayerId;
        Health = state.Health;
        MaxHealth = state.MaxHealth;
        GettingRepaired = state.GettingRepaired;
        Type = state.Type;
        RenderPosition = state.CurrentPosition;
        TargetPosition = state.TargetPosition;
        MovementSpeed = state.MovementSpeed;
    }
    public override void _Process(double delta)
    {
        if (Math.Max(GlobalPosition.DistanceTo(RenderPosition), 0) <= 3f)
        {
            return; 
        }
        LookAt(TargetPosition);
        GlobalPosition = GlobalPosition.MoveToward(RenderPosition, (float) (MovementSpeed * delta));
    }
}
