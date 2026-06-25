using Godot;
using System;

/*
Implementation of the render BuildingState
*/
public partial class Barracks : ClientBuilding
{
	private ProgressBar BuildProgressBar = null;
	private ProgressBar ProductionProgressBar = null;

	public override void _Ready()
	{
		BuildProgressBar = GetNode<ProgressBar>("BodyRender/BuildProgressBar");
		BuildProgressBar.MaxValue = 100;
		BuildProgressBar.Value = 0;

		ProductionProgressBar = GetNode<ProgressBar>("BodyRender/ProductionProgressBar");
		ProductionProgressBar.Visible = false;
		ProductionProgressBar.Value = 0;
	}

	public override void ApplyState(BuildingState state)
	{
		base.ApplyState(state);

		BuildProgressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			BuildProgressBar.Visible = false;

		if (state.ProductionProgress > 0)
		{
			ProductionProgressBar.Visible = true;
			ProductionProgressBar.Value = state.ProductionProgress;
		} else
		{
			ProductionProgressBar.Visible = false;
			ProductionProgressBar.Value = 0;
		}
	}

}
