using Godot;

public partial class CameraController : Camera2D
{
	[Export] public float MinZoom { get; set; } = 0.5f;
	[Export] public float MaxZoom { get; set; } = 2.5f;
	[Export] public float ZoomStep { get; set; } = 0.1f;

	private bool isDraggingCamera = false;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
			{
				isDraggingCamera = mouseButtonEvent.Pressed;
				GetViewport().SetInputAsHandled();
				return;
			}

			if (mouseButtonEvent.Pressed &&
				(mouseButtonEvent.ButtonIndex == MouseButton.WheelUp ||
				mouseButtonEvent.ButtonIndex == MouseButton.WheelDown))
			{
				var zoomDelta = mouseButtonEvent.ButtonIndex == MouseButton.WheelUp
					? ZoomStep
					: -ZoomStep;

				SetZoomLevel(Zoom.X + zoomDelta);
				GetViewport().SetInputAsHandled();
				return;
			}
		}

		if (@event is InputEventMouseMotion mouseMotionEvent && isDraggingCamera)
		{
			Position -= mouseMotionEvent.Relative / Zoom;
			GetViewport().SetInputAsHandled();
		}
	}

	private void SetZoomLevel(float zoomLevel)
	{
		var clampedZoom = Mathf.Clamp(zoomLevel, MinZoom, MaxZoom);
		Zoom = new Vector2(clampedZoom, clampedZoom);
	}
}
