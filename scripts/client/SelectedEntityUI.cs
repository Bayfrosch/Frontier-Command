using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SelectedEntityUI : Control
{
	[Signal]
	public delegate void AbilityPressedEventHandler(string abilityId, bool requiresTarget);
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
		row.AddThemeConstantOverride("separation", 16);

		foreach (var child in row.GetChildren())
		{
			child.QueueFree();
		}

		if (SelectedEntityIds.Count <= 0)
		{
			return;
		}

		if (!EntitiesById.TryGetValue(SelectedEntityIds.First(), out var priorizedEntity))
			return;

		if (SelectedEntityIds.Count > 1)
		{
			var selectedUnits = SelectedEntityIds
				.Select(id => EntitiesById.TryGetValue(id, out var entity) ? entity : null)
				.OfType<UnitState>()
				.ToList();

			var capitalUnit = selectedUnits
				.FirstOrDefault(unit => UnitCatalog.GetCapitalUnits().Contains(unit.Type));

			if (capitalUnit is not null)
			{
				priorizedEntity = capitalUnit;
			}
			else if (selectedUnits.Count > 0)
			{
				var mostCommonType = selectedUnits
					.GroupBy(unit => unit.Type)
					.OrderByDescending(group => group.Count())
					.First()
					.Key;

				priorizedEntity = selectedUnits.First(unit => unit.Type == mostCommonType);
			}
		}

		if (priorizedEntity.Abilities is null || priorizedEntity.Abilities.Count <= 0)
		{
			return;
		}

			foreach (Ability ability in priorizedEntity.Abilities)
			{
				/*
				Rendering of each Ability in bottom Row
				Generates a Button for each Ability
				*/
				var AbilityButton = new Button
				{
					Text = ability.Name,
					CustomMinimumSize = new Vector2(140, 60),
				};

				var normalStyle = new StyleBoxFlat
				{
					BgColor = new Color(0.5f, 0.5f, 0.5f),
					CornerRadiusBottomLeft = 8,
					CornerRadiusBottomRight = 8,
					CornerRadiusTopLeft = 8,
					CornerRadiusTopRight = 8,
				};

				var hoverStyle = (StyleBoxFlat)normalStyle.Duplicate();
				hoverStyle.BgColor = new Color(0.3f,0.3f,0.3f);
				
				var pressedStyle = (StyleBoxFlat)normalStyle.Duplicate();
				pressedStyle.BgColor = new Color(0.1f,0.1f,0.1f);

				AbilityButton.AddThemeStyleboxOverride("normal", normalStyle);
				AbilityButton.AddThemeStyleboxOverride("hover", hoverStyle);
				AbilityButton.AddThemeStyleboxOverride("pressed", pressedStyle);
				
				AbilityButton.Pressed += () =>
				{
					EmitSignal(SignalName.AbilityPressed, ability.Id, ability.RequiresTarget);
				};

				row.AddChild(AbilityButton);
			}
	}
}
