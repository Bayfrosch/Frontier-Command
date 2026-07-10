using Godot;

public partial class CameraController : Camera2D
{
	private bool isDraggingCamera = false;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent &&
			mouseButtonEvent.ButtonIndex == MouseButton.Middle)
		{
			isDraggingCamera = mouseButtonEvent.Pressed;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseMotion mouseMotionEvent && isDraggingCamera)
		{
			Position -= mouseMotionEvent.Relative / Zoom;
			GetViewport().SetInputAsHandled();
		}
	}
}
