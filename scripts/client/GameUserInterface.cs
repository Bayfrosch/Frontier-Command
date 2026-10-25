using Godot;

public partial class GameUserInterface : Control
{
	public ResearchOverlay Research { get; private set; }
	private Control settings = null;
	private OptionButton resolutionOptions;
	private Button cheatModeButton = null;
	private LocalSimulationNode cheatSimulation = null;
	private string cheatPlayerId = "";
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
		var researchButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/ResearchButton");
		Research = GetNode<ResearchOverlay>("Research");
		researchButton.Pressed += () =>
		{
			settings.Hide();
			Research.Show();
			Research.GetNode<Button>("ResearchPanel/Margin/Layout/Header/CloseButton").GrabFocus();
		};
		Research.Closed += () => researchButton.GrabFocus();
		var settingsButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/SettingsButton");
		settings = GetNode<Control>("Settings");
		var closeButton = GetNode<Button>("Settings/SettingsPanel/MarginContainer/VBoxContainer/ButtonRow/CloseButton");
		resolutionOptions = GetNode<OptionButton>("Settings/SettingsPanel/MarginContainer/VBoxContainer/ResolutionRow/ResolutionOptions");
		cheatModeButton = GetNode<Button>("Settings/SettingsPanel/MarginContainer/VBoxContainer/CheatModeButton");
		RefreshResolutions();
		resolutionOptions.ItemSelected += ApplyResolution;
		GetWindow().SizeChanged += RefreshResolutions;

		settingsButton.Pressed += ToggleSettings;
		closeButton.Pressed += HideSettings;
		cheatModeButton.Pressed += ToggleCheatMode;
	}
	public void BindSimulation(LocalSimulationNode simulation, string playerId)
	{
		if (cheatSimulation is not null)
			cheatSimulation.StateChanged -= RefreshCheatMode;

		cheatSimulation = simulation;
		cheatPlayerId = playerId;
		cheatSimulation.StateChanged += RefreshCheatMode;
		RefreshCheatMode();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (Research.Visible && @event.IsActionPressed("ui_cancel"))
		{
			Research.Close();
			GetViewport().SetInputAsHandled();
		}
	}

	private void ToggleSettings()
	{
		if (!settings.Visible)
		{
			RefreshResolutions();
			RefreshCheatMode();
		}
		settings.Visible = !settings.Visible;
	}
	private void ToggleCheatMode()
	{
		if (cheatSimulation is null
			|| !cheatSimulation.GetState().Players.TryGetValue(cheatPlayerId, out var player))
		{
			RefreshCheatMode();
			return;
		}

		cheatSimulation.Push(new SetCheatModeMessage(
			cheatPlayerId,
			cheatSimulation.GetState().Tick,
			!player.CheatModeEnabled
		));
		RefreshCheatMode();
	}
	private void RefreshCheatMode()
	{
		var enabled = cheatSimulation is not null
			&& cheatSimulation.GetState().Players.TryGetValue(cheatPlayerId, out var player)
			&& player.CheatModeEnabled;
		cheatModeButton.ButtonPressed = enabled;
		cheatModeButton.Text = enabled ? "Cheat Mode: Enabled" : "Cheat Mode: Disabled";
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
		if (cheatSimulation is not null)
			cheatSimulation.StateChanged -= RefreshCheatMode;
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
