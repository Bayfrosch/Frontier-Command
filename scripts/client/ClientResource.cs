using Godot;
using System.Collections.Generic;

public partial class ClientResource : Node2D, ClientEntity
{
	public string EntityId { get; private set; } = "";
	public string? OwnerPlayerId { get; private set; }
	public int Health { get; private set; }
	public int MaxHealth { get; private set; }
	public bool GettingRepaired { get; private set; }
	public HashSet<Ability> Abilities { get; private set; } = new();
	public ResourceType ResourceType { get; private set; }
	public int CurrentAmount { get; private set; }
	public int MaxAmount { get; private set; }
	private ProgressBar AmountBar = null;

	public override void _Ready()
	{
		ClientWorldInput.IgnoreGuiMouse(this);

		AmountBar = GetNode<ProgressBar>("AmountBar");
	}

	public void ApplySpawnState(ResourceState state)
	{
		ApplyState(state);
		Position = state.CurrentPosition;
		AmountBar.MaxValue = MaxAmount;
	}

	public void ApplyState(ResourceState state)
	{
		EntityId = state.EntityId;
		OwnerPlayerId = state.OwnerPlayerId;
		Health = state.Health;
		MaxHealth = state.MaxHealth;
		GettingRepaired = state.GettingRepaired;
		ResourceType = state.ResourceType;
		CurrentAmount = state.CurrentAmount;
		MaxAmount = state.MaxAmount;
		AmountBar.Value = CurrentAmount;
	}
}
