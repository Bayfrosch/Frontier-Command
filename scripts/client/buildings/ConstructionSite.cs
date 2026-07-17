using Godot;

public partial class ConstructionSite : ClientBuilding
{
	private ColorRect BodyRender = null;
	private ProgressBar BuildProgressBar = null;
	private CollisionShape2D CollisionShape = null;

    public override void _Ready()
    {
		base._Ready();

		BodyRender = GetNode<ColorRect>("BodyRender");
		BuildProgressBar = GetNode<ProgressBar>("BodyRender/BuildProgressBar");
		CollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		BuildProgressBar.MaxValue = 100;
		BuildProgressBar.Value = 0;
    }

    public override void ApplyState(BuildingState state)
    {
        base.ApplyState(state);

		ApplyFootprintSize(state.pendingBuilding);
		BuildProgressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			BuildProgressBar.Visible = false;

    }

	private void ApplyFootprintSize(BuildingType pendingBuildingType)
	{
		var footprintSize = BuildingCatalog.GetFoodprintSize(pendingBuildingType);

		BodyRender.OffsetLeft = -footprintSize.X / 2f;
		BodyRender.OffsetTop = -footprintSize.Y / 2f;
		BodyRender.OffsetRight = footprintSize.X / 2f;
		BodyRender.OffsetBottom = footprintSize.Y / 2f;

		BuildProgressBar.OffsetLeft = -footprintSize.X / 2f + 5f;
		BuildProgressBar.OffsetRight = footprintSize.X / 2f - 5f;

		if (CollisionShape.Shape is RectangleShape2D rectangleShape)
		{
			var siteShape = rectangleShape.Duplicate() as RectangleShape2D;
			if (siteShape is null)
				return;

			siteShape.Size = footprintSize;
			CollisionShape.Shape = siteShape;
		}
	}
}
