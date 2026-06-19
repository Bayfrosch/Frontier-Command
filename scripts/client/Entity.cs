using Godot;
using System;

public interface ClientEntity
{
    string EntityId {get; }
    string? OwnerPlayerId { get; }
    int Health { get; }
    int MaxHealth { get; }
    bool GettingRepaired { get; }
}
