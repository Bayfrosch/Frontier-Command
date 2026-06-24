using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SelectedEntityUI : Control
{
	private IReadOnlyCollection<string> SelectedEntityIds;
	private IReadOnlyDictionary<string, EntityState> EntitiesById;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameScene gameScene = GetTree().CurrentScene as GameScene;
		if (gameScene is null)
		{
			GD.Print("GameScene not found in SelectedEntityUI.");
			return;
		}

		SelectedEntityIds = gameScene.SelectedEntityIds;
		gameScene.UnitSelection += HandleUnitSelection;

		LocalSimulationNode simulationNode = gameScene.GetNode<LocalSimulationNode>("SimulationCore");
		if (simulationNode is null)
		{
			GD.Print("LocalSimulationNode is null in SelectedEntityUI.");
			return;
		}
		if (!simulationNode.Context.get().Players.TryGetValue("player_1", out var player))
			return;

		EntitiesById = player.Entities;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void HandleUnitSelection()
	{
		var row = GetNode<HBoxContainer>("VBoxContainer/Row1");

		foreach (var child in row.GetChildren())
		{
			child.QueueFree();
		}

		if (SelectedEntityIds.Count <= 0)
		{
			return;
		}

		var selectedId = SelectedEntityIds.First();
		if (!EntitiesById.TryGetValue(selectedId, out var priorizedEntity))
			return;

		if (SelectedEntityIds.Count > 1)
		{
			// TODO: Select priority Unit (Special first, most second)
		}

		if (priorizedEntity.Abilities is null || priorizedEntity.Abilities.Count <= 0)
		{
			return;
		}

		foreach (Ability ability in priorizedEntity.Abilities)
		{
			// TODO: Render each Ability
			var Label = new Label
			{
				Text = ability.Name,
			};
			row.AddChild(Label);
		}
	}
}
