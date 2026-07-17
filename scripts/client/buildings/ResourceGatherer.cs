using Godot;

public partial class ResourceGatherer : ClientBuilding
{
	private ProgressBar ProductionProgressBar = null;

	public override void _Ready()
	{
		base._Ready();

		ProductionProgressBar = GetNode<ProgressBar>("BodyRender/ProductionProgressBar");
		ProductionProgressBar.Visible = false;
		ProductionProgressBar.Value = 0;
	}

	public override void ApplyState(BuildingState state)
	{
		base.ApplyState(state);
		ApplyProductionProgress(ProductionProgressBar, state);
	}
}
