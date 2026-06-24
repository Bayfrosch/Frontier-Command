using Godot;
using System;
using System.Collections.Generic;

/*
Entity object used in Client to render entities
*/
public interface ClientEntity
{
    string EntityId { get; }
    string? OwnerPlayerId { get; }
    int Health { get; }
    int MaxHealth { get; }
    bool GettingRepaired { get; }
    HashSet<Ability> Abilities { get; }
}
