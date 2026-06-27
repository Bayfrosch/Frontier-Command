using Godot;

public partial class ConstructionSite : ClientBuilding
{
	private ProgressBar BuildProgressBar = null;

    public override void _Ready()
    {
		BuildProgressBar = GetNode<ProgressBar>("BodyRender/BuildProgressBar");
		BuildProgressBar.MaxValue = 100;
		BuildProgressBar.Value = 0;
    }

    public override void ApplyState(BuildingState state)
    {
        base.ApplyState(state);

		BuildProgressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			BuildProgressBar.Visible = false;

    }
}