using Godot;
using System;

/*
Implementation of the render BuildingState
*/
public partial class TestBuilding : ClientBuilding
{
	private ProgressBar progressBar = null!;

	public override void _Ready()
	{
		progressBar = GetNode<ProgressBar>("BodyRender/ProgressBar");
		progressBar.MaxValue = 100;
		progressBar.Value = 0;
	}

	public override void ApplyState(BuildingState state)
	{
		base.ApplyState(state);

		progressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			progressBar.Visible = false;
	}

}
