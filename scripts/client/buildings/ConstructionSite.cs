using Godot;

public partial class ConstructionSite : ClientBuilding
{
	private ProgressBar BuildProgressBar = null;
	private BuildingType? footprintType;

    public override void _Ready()
    {
		base._Ready();

		BuildProgressBar = GetNode<ProgressBar>("BodyRender/BuildProgressBar");
		BuildProgressBar.MaxValue = 100;
		BuildProgressBar.Value = 0;
    }

    public override void ApplyState(BuildingState state)
    {
        base.ApplyState(state);

		ApplyFootprint(state.pendingBuilding);
		BuildProgressBar.Value = state.BuildProgression;
		if (state.BuildProgression >= 100)
			BuildProgressBar.Visible = false;

    }

	// Also used by placement previews before the node enters the scene tree.
	public void ApplyFootprint(BuildingType pendingBuildingType)
	{
		if (footprintType == pendingBuildingType)
			return;

		var footprint = BuildingCatalog.GetFootprintRect(pendingBuildingType, Vector2.Zero);
		var body = GetNode<ColorRect>("BodyRender");
		var progressBar = GetNode<ProgressBar>("BodyRender/BuildProgressBar");
		var collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");

		body.OffsetLeft = footprint.Position.X;
		body.OffsetTop = footprint.Position.Y;
		body.OffsetRight = footprint.End.X;
		body.OffsetBottom = footprint.End.Y;

		progressBar.OffsetLeft = -footprint.Size.X / 2f + 5f;
		progressBar.OffsetRight = footprint.Size.X / 2f - 5f;
		collisionShape.Position = footprint.GetCenter();

		if (collisionShape.Shape is RectangleShape2D rectangleShape)
		{
			var siteShape = rectangleShape.Duplicate() as RectangleShape2D;
			if (siteShape is null)
				return;

			siteShape.Size = footprint.Size;
			collisionShape.Shape = siteShape;
		}
		footprintType = pendingBuildingType;
	}
}
