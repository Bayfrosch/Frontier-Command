using Godot;
using System;

/*
Implementation of the render BuildingState
*/
public partial class Barracks : ClientBuilding
{
	private ProgressBar ProductionProgressBar = null;

	public override void _Ready()
	{

		ProductionProgressBar = GetNode<ProgressBar>("BodyRender/ProductionProgressBar");
		ProductionProgressBar.Visible = false;
		ProductionProgressBar.Value = 0;
	}

	public override void ApplyState(BuildingState state)
	{
		base.ApplyState(state);

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
