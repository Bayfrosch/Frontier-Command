using Godot;

public partial class GameUserInterface : Control
{
	private Control settings = null;

	public override void _Ready()
	{
		var settingsButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/HBoxContainer/SettingsButton");
		settings = GetNode<Control>("Settings");
		var closeButton = GetNode<Button>("Settings/SettingsPanel/MarginContainer/VBoxContainer/ButtonRow/CloseButton");

		settingsButton.Pressed += ToggleSettings;
		closeButton.Pressed += HideSettings;
	}

	private void ToggleSettings()
	{
		settings.Visible = !settings.Visible;
	}

	private void HideSettings()
	{
		settings.Visible = false;
	}
}
