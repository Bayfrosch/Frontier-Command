using Godot;
using System;

public partial class GUI : Control
{
	[Signal]
	public delegate void SpawnPressedEventHandler();
	
	private Button _spawnButton;

	public override void _Ready()
	{
		_spawnButton = GetNode<Button>("MainLayout/VBoxContainer/TopBar/SpawnButton");
		_spawnButton.Pressed += OnSpawnButtonPressed;
	}

	private void OnSpawnButtonPressed()
	{
		EmitSignal(SignalName.SpawnPressed);
	}
}
