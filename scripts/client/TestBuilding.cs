using Godot;
using System;

public partial class TestBuilding : ClientBuilding
{
	private ProgressBar progressBar = null!;
	public void Initialize(string p_entityId, BuildingType p_Type, Vector2 p_Position, string? p_OwnerPlayerId = "")
	{
		EntityId = p_entityId;
		Type = p_Type;
		OwnerPlayerId = p_OwnerPlayerId;
		// Handle various Building healths
		MaxHealth = Type switch
		{
			BuildingType.BASIC_GENERATOR => 100,
			_ => 100,
		};
		Health = 0;
		Position = p_Position;
	}
	public override void _Ready()
	{
		progressBar = GetNode<ProgressBar>("BodyRender/ProgressBar");
		progressBar.MaxValue = 100;
		progressBar.Value = 0;
	}

	private void Repair()
	{
		Health = Math.Min(MaxHealth, Health + REPAIR_RATE);
	}

	public override void ApplyState(BuildingState state)
	{
		base.ApplyState(state);

		progressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			progressBar.Visible = false;
	}

}
