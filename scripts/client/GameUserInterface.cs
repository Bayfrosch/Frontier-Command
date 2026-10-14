using Godot;

public partial class GameUserInterface : Control
{
	private Control settings = null;
	private OptionButton resolutionOptions;
	private readonly Vector2I[] resolutions =
	{
		new(1280, 720),
		new(1600, 900),
		new(1920, 1080),
		new(2560, 1440),
		new(3840, 2160),
	};

	public override void _Ready()
	{
		var settingsButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/SettingsButton");
		settings = GetNode<Control>("Settings");
		var closeButton = GetNode<Button>("Settings/SettingsPanel/MarginContainer/VBoxContainer/ButtonRow/CloseButton");
		resolutionOptions = GetNode<OptionButton>("Settings/SettingsPanel/MarginContainer/VBoxContainer/ResolutionRow/ResolutionOptions");
		RefreshResolutions();
		resolutionOptions.ItemSelected += ApplyResolution;
		GetWindow().SizeChanged += RefreshResolutions;

		settingsButton.Pressed += ToggleSettings;
		closeButton.Pressed += HideSettings;
	}

	private void ToggleSettings()
	{
		if (!settings.Visible)
			RefreshResolutions();
		settings.Visible = !settings.Visible;
	}

	private void RefreshResolutions()
	{
		resolutionOptions.Clear();
		var currentSize = GetWindow().Size;
		var selected = -1;
		for (var i = 0; i < resolutions.Length; i++)
		{
			var size = resolutions[i];
			resolutionOptions.AddItem($"{size.X} × {size.Y}");
			if (size == currentSize)
				selected = i;
		}
		if (selected == -1)
		{
			resolutionOptions.AddItem($"{currentSize.X} × {currentSize.Y} (current)");
			selected = resolutions.Length;
		}
		resolutionOptions.Select(selected);
	}

	public override void _ExitTree()
	{
		GetWindow().SizeChanged -= RefreshResolutions;
	}

	private async void ApplyResolution(long index)
	{
		if (index < 0 || index >= resolutions.Length)
			return;

		var window = GetWindow();
		window.Unresizable = false;
		window.Mode = Window.ModeEnum.Windowed;
		// Let the window manager finish leaving maximized/fullscreen mode first.
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (!IsInsideTree())
			return;
		window.Size = resolutions[(int)index];
	}

	private void HideSettings()
	{
		settings.Visible = false;
	}
}
