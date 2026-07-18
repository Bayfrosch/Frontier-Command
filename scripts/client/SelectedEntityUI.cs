using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SelectedEntityUI : Control
{
	private const string SellBuildingAbilityId = "sell_building";
	[Signal]
	public delegate void AbilityPressedEventHandler(string abilityId, bool requiresTarget);
	private IReadOnlyCollection<string> SelectedEntityIds;
	private IReadOnlyDictionary<string, EntityState> EntitiesById;
	private PlayerState LocalPlayer;
	private int lastAbilityUnlockRevision = -1;

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

		LocalPlayer = player;
		EntitiesById = player.Entities;
		lastAbilityUnlockRevision = player.AbilityUnlockRevision;
		simulationNode.StateChanged += HandleStateChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void HandleStateChanged()
	{
		if (LocalPlayer is null)
			return;

		if (lastAbilityUnlockRevision == LocalPlayer.AbilityUnlockRevision)
			return;

		HandleUnitSelection();
	}

	private void HandleUnitSelection()
	{
		if (LocalPlayer is not null)
			lastAbilityUnlockRevision = LocalPlayer.AbilityUnlockRevision;

		var row1 = GetNode<HBoxContainer>("VBoxContainer/Row1");
		var row2 = GetNode<HBoxContainer>("VBoxContainer/Row2");
		row1.AddThemeConstantOverride("separation", 16);
		row2.AddThemeConstantOverride("separation", 16);

		ClearRow(row1);
		ClearRow(row2);

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

		foreach (Ability ability in priorizedEntity.Abilities.Where(ability => ability.Id != SellBuildingAbilityId))
		{
			row1.AddChild(CreateAbilityButton(ability));
		}

		var sellAbility = priorizedEntity.Abilities.FirstOrDefault(ability => ability.Id == SellBuildingAbilityId);
		if (sellAbility is not null)
		{
			row2.AddChild(new Control
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				MouseFilter = MouseFilterEnum.Ignore
			});
			row2.AddChild(CreateAbilityButton(sellAbility));
		}
	}

	private static void ClearRow(HBoxContainer row)
	{
		foreach (var child in row.GetChildren())
			child.QueueFree();
	}

	private Button CreateAbilityButton(Ability ability)
	{
		var isUnlocked = LocalPlayer.UnlockedAbilities.Contains(ability.Id);
		var abilityButton = new Button
		{
			Text = ability.Name,
			CustomMinimumSize = new Vector2(140, 60),
		};
		var normalStyle = new StyleBoxFlat
		{
			BgColor = isUnlocked ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.2f, 0.2f, 0.2f),
			CornerRadiusBottomLeft = 8,
			CornerRadiusBottomRight = 8,
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8,
		};

		var hoverStyle = (StyleBoxFlat)normalStyle.Duplicate();
		hoverStyle.BgColor = isUnlocked ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.2f, 0.2f, 0.2f);

		var pressedStyle = (StyleBoxFlat)normalStyle.Duplicate();
		pressedStyle.BgColor = isUnlocked ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.2f, 0.2f, 0.2f);

		abilityButton.AddThemeStyleboxOverride("normal", normalStyle);
		abilityButton.AddThemeStyleboxOverride("hover", hoverStyle);
		abilityButton.AddThemeStyleboxOverride("pressed", pressedStyle);

		abilityButton.Pressed += () =>
		{
			EmitSignal(SignalName.AbilityPressed, ability.Id, ability.RequiresTarget);
		};

		return abilityButton;
	}
}
