using Godot;

public partial class GameUserInterface : Control
{
	private Control settingsOverlay = null;

	public override void _Ready()
	{
		var settingsButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/SettingsButton");
		settingsOverlay = GetNode<Control>("SettingsOverlay");
		var closeButton = GetNode<Button>("SettingsOverlay/SettingsPanel/MarginContainer/VBoxContainer/ButtonRow/CloseButton");

		settingsButton.Pressed += ToggleSettings;
		closeButton.Pressed += HideSettings;
	}

	private void ToggleSettings()
	{
		settingsOverlay.Visible = !settingsOverlay.Visible;
	}

	private void HideSettings()
	{
		settingsOverlay.Visible = false;
	}
}
