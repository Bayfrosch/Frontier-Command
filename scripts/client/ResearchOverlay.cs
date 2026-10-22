using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResearchOverlay : Control
{
	[Signal]
	public delegate void ClosedEventHandler();

	private const string DetailPath = "ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll/Content/";
	private static readonly Vector2 CardSize = new(240, 64);
	private static readonly Vector2 CardStep = new(288, 100);
	private static readonly Dictionary<ResearchChapter, string[]> Trees = new()
	{
		[ResearchChapter.Military] = ["Infantry", "Tanks", "Aircraft"],
		[ResearchChapter.Civilian] = ["Extraction", "Energy", "Infrastructure"],
		[ResearchChapter.Fleet] = ["Fleet capacity", "Capital capacity"]
	};
	private readonly Dictionary<string, Button> cards = new();
	private readonly Dictionary<ResearchChapter, Control> canvases = new();
	private readonly List<(Label Label, ResearchDefinition[] Technologies)> treeProgress = new();
	private readonly HashSet<string> completedCards = new();
	private readonly StyleBoxFlat completedNormal = CardStyle(new Color("17392b"), new Color("69c991"));
	private readonly StyleBoxFlat completedHover = CardStyle(new Color("224c36"), new Color("90e5b0"));
	private readonly StyleBoxFlat completedSelected = CardStyle(new Color("24573b"), new Color("edc77f"));
	private Label title;
	private Label category;
	private Label description;
	private Label cost;
	private Label requirements;
	private Label status;
	private Label balance;
	private TabBar chapters;
	private ScrollContainer treeScroll;
	private Button startButton;
	private Button cancelButton;
	private ResearchDefinition selected;
	private LocalSimulationNode simulation;
	private ProgressBar progress;

	public override void _Ready()
	{
		title = GetNode<Label>(DetailPath + "TechnologyName");
		category = GetNode<Label>(DetailPath + "Category");
		description = GetNode<Label>(DetailPath + "Description");
		cost = GetNode<Label>(DetailPath + "Cost");
		requirements = GetNode<Label>(DetailPath + "Requirements");
		status = GetNode<Label>(DetailPath + "Status");
		balance = GetNode<Label>("ResearchPanel/Margin/Layout/Header/Balance");
		chapters = GetNode<TabBar>("ResearchPanel/Margin/Layout/Chapters");
		treeScroll = GetNode<ScrollContainer>("ResearchPanel/Margin/Layout/Body/TreeScroll");
		startButton = GetNode<Button>(DetailPath + "StartButton");
		cancelButton = GetNode<Button>(DetailPath + "CancelButton");
		progress = GetNode<ProgressBar>(DetailPath + "Progress");
		GetNode<Button>("ResearchPanel/Margin/Layout/Header/CloseButton").Pressed += Close;
		startButton.Pressed += StartResearch;
		cancelButton.Pressed += CancelResearch;
		VisibilityChanged += RefreshProjects;
		foreach (var chapter in Enum.GetValues<ResearchChapter>())
			chapters.AddTab(chapter.ToString());
		BuildTrees();
		chapters.TabChanged += index => ShowChapter((ResearchChapter)index);
		ShowChapter(ResearchChapter.Military);
	}

	public void BindSimulation(LocalSimulationNode value)
	{
		simulation = value;
		simulation.StateChanged += RefreshProjects;
		RefreshProjects();
	}

	public override void _ExitTree()
	{
		if (simulation is not null)
			simulation.StateChanged -= RefreshProjects;
	}

	public void Close()
	{
		Hide();
		EmitSignal(SignalName.Closed);
	}

	private void ShowChapter(ResearchChapter chapter)
	{
		foreach (var (key, canvas) in canvases)
			canvas.Visible = key == chapter;
		treeScroll.ScrollVertical = 0;
		treeScroll.ScrollHorizontal = 0;
		SelectTechnology(ResearchCatalog.All.First(technology => technology.Chapter == chapter));
	}

	private void BuildTrees()
	{
		var group = new ButtonGroup();
		foreach (var chapter in Enum.GetValues<ResearchChapter>())
		{
			var technologies = ResearchCatalog.All.Where(technology => technology.Chapter == chapter).ToArray();
			var canvas = new ResearchTreeCanvas { Name = chapter + "Tree", Chapter = chapter, MouseFilter = MouseFilterEnum.Pass };
			treeScroll.GetNode<CenterContainer>("TreeCenter").AddChild(canvas);
			canvases.Add(chapter, canvas);
			var canvasWidth = (chapter == ResearchChapter.Military ? 4 : 3) * CardStep.X;
			float y = 76;
			for (var generation = 1; generation <= 3; generation++)
			{
				AddTierBoundary(canvas, generation, generation == 1 ? 12 : y - 40, canvasWidth);
				var rows = 0;
				for (var column = 0; column < Trees[chapter].Length; column++)
				{
					var tree = Trees[chapter][column];
					var nodes = technologies.Where(technology => technology.Tree == tree && technology.Generation == generation).ToArray();
					var mainNodes = nodes.Where(technology => technology.Id != "better_weapons").ToArray();
					rows = Math.Max(rows, mainNodes.Length);
					var x = 24 + column * CardStep.X + (chapter == ResearchChapter.Military ? CardStep.X
						: chapter == ResearchChapter.Fleet ? CardStep.X / 2 : 0);
					if (generation == 1)
					{
						var heading = new Label
						{
							Position = new Vector2(x, y - 32), Text = tree,
							MouseFilter = MouseFilterEnum.Ignore
						};
						heading.AddThemeColorOverride("font_color", ChapterColor(chapter));
						canvas.AddChild(heading);
						treeProgress.Add((heading, nodes));
					}
					for (var row = 0; row < nodes.Length; row++)
					{
						var technology = nodes[row];
						var position = technology.Id == "better_weapons"
							? new Vector2(x - CardStep.X, y)
							: new Vector2(x, y + Array.IndexOf(mainNodes, technology) * CardStep.Y);
						canvas.Positions.Add(technology.Id, position);
						var card = new Button
						{
							Name = technology.Id, Position = position, Size = CardSize,
							ToggleMode = true, ButtonGroup = group, Text = technology.Name,
							TooltipText = technology.Description, MouseDefaultCursorShape = CursorShape.PointingHand
						};
						var doctrine = technology.Id is "infantry_doctrine" or "tank_doctrine";
						var accent = doctrine ? new Color("e3a76a") : ChapterColor(chapter);
						card.AddThemeStyleboxOverride("normal", CardStyle(doctrine ? new Color("382b22") : new Color("202421"),
							doctrine ? accent : accent.Darkened(0.5f)));
						card.AddThemeStyleboxOverride("hover", CardStyle(new Color("2d322c"), accent));
						card.AddThemeStyleboxOverride("pressed", CardStyle(new Color("35392e"), new Color("edc77f")));
						card.AddThemeStyleboxOverride("focus", CardStyle(Colors.Transparent, new Color("edc77f")));
						card.AddThemeFontSizeOverride("font_size", 16);
						card.Pressed += () => SelectTechnology(technology);
						canvas.AddChild(card);
						cards.Add(technology.Id, card);
					}
				}
				if (chapter == ResearchChapter.Military && generation == 2)
				{
					var eitherOr = new Label
					{
						Name = "DoctrineEitherOr", Position = new Vector2(556, y + 18), Size = new Vector2(40, 28),
						Text = "OR", HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore
					};
					eitherOr.AddThemeColorOverride("font_color", new Color("e3a76a"));
					eitherOr.AddThemeFontSizeOverride("font_size", 20);
					canvas.AddChild(eitherOr);
				}
				y += rows * CardStep.Y + 40;
			}
			canvas.CustomMinimumSize = new Vector2(canvasWidth, y);
		}
	}

	private static void AddTierBoundary(Control canvas, int generation, float y, float width)
	{
		canvas.AddChild(new ColorRect
		{
			Name = $"Generation{generation}Divider",
			Position = new Vector2(152, y + 13), Size = new Vector2(width - 176, 2),
			Color = new Color("777052"), MouseFilter = MouseFilterEnum.Ignore
		});
		var number = generation switch { 1 => "I", 2 => "II", _ => "III" };
		var label = new Label
		{
			Name = $"Generation{generation}Label", Position = new Vector2(24, y), Size = new Vector2(120, 26),
			Text = $"Generation {number}", HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore
		};
		label.AddThemeColorOverride("font_color", new Color("b8b7a7"));
		label.AddThemeFontSizeOverride("font_size", 14);
		canvas.AddChild(label);
	}

	private void SelectTechnology(ResearchDefinition technology)
	{
		selected = technology;
		cards[technology.Id].SetPressedNoSignal(true);
		title.Text = technology.Name;
		category.Text = $"{technology.Chapter.ToString().ToUpperInvariant()}  /  {technology.Tree}";
		category.AddThemeColorOverride("font_color", ChapterColor(technology.Chapter));
		description.Text = technology.Description;
		var prerequisites = technology.Prerequisites.Select(id => ResearchCatalog.All.First(item => item.Id == id).Name);
		requirements.Text = "Prerequisites\n" + (technology.Prerequisites.Length == 0
			? "No earlier technology required." : string.Join(", ", prerequisites));
		if (technology.Id is "infantry_doctrine" or "tank_doctrine")
		{
			var other = technology.Tree == "Infantry" ? "Tanks" : "Infantry";
			requirements.Text = $"Prerequisites\nAny 2 Military core trees and any 1 Civilian core tree.\nCompleting this doctrine permanently excludes the {other} doctrine and its exclusive technologies.";
		}
		GetNode<ScrollContainer>("ResearchPanel/Margin/Layout/Body/Details/Margin/Scroll").ScrollVertical = 0;
		RefreshProjects();
	}

	private PlayerState LocalPlayer => simulation?.GetState().Players.GetValueOrDefault("player_1");

	private static BuildingState ResearchHost(PlayerState player) => player.Entities.Values
		.OfType<BuildingState>().FirstOrDefault(building => building.Type == BuildingType.COMMAND_CENTER && building.Health > 0);

	private void RefreshProjects()
	{
		var player = LocalPlayer;
		if (player is null || selected is null)
		{
			startButton.Disabled = true;
			return;
		}
		balance.Text = $"Materials: {player.Virelium:N0}";
		var military = ResearchCatalog.CompletedCoreTrees(player, ResearchChapter.Military);
		var civilian = ResearchCatalog.CompletedCoreTrees(player, ResearchChapter.Civilian);
		var chosenDoctrine = ResearchCatalog.All.FirstOrDefault(technology => (technology.Id is "infantry_doctrine" or "tank_doctrine")
			&& (player.Research.ContainsKey(technology.Id) || player.ResearchProjects.ContainsKey(technology.Id)));
		var doctrinePending = chosenDoctrine is not null && player.ResearchProjects.ContainsKey(chosenDoctrine.Id);
		var militaryCanvas = (ResearchTreeCanvas)canvases[ResearchChapter.Military];
		if (militaryCanvas.ChosenDoctrine != chosenDoctrine)
		{
			militaryCanvas.ChosenDoctrine = chosenDoctrine;
			militaryCanvas.QueueRedraw();
		}
		foreach (var (label, technologies) in treeProgress)
			label.Text = $"{technologies[0].Tree}  {technologies.Count(technology => player.Research.ContainsKey(technology.Id))}/{technologies.Length}";
		player.ResearchProjects.TryGetValue(selected.Id, out var project);
		var selectedLevel = player.GetResearchLevel(selected.Id);
		var completed = selectedLevel >= selected.MaxLevel;
		var nextLevel = Math.Min(selectedLevel + 1, selected.MaxLevel);
		var materialCost = selected.GetMaterialCost(nextLevel);
		cost.Text = completed && selected.MaxLevel > 1 ? $"All {selected.MaxLevel} levels researched."
			: $"{(selected.MaxLevel > 1 ? $"Level {nextLevel}/{selected.MaxLevel}\n" : "")}{materialCost:N0} Materials\nPlanned power: +{selected.Energy}\nDuration: {selected.Seconds} seconds";
		var busy = player.ResearchProjects.Values.FirstOrDefault(item => item.Chapter == selected.Chapter);
		var locked = ResearchCatalog.LockReason(player, selected);
		var excluded = IsOtherDoctrinePath(selected, chosenDoctrine);
		var selectingDoctrine = selected.Id is "infantry_doctrine" or "tank_doctrine";
		startButton.Visible = project is null;
		startButton.Text = completed ? "Research complete" : selectingDoctrine ? $"Choose {selected.Tree} doctrine"
			: selected.MaxLevel > 1 ? $"Research level {nextLevel}" : "Start research";
		startButton.Disabled = completed || busy is not null || locked.Length > 0 || excluded
			|| player.Virelium < materialCost || ResearchHost(player) is null;
		cancelButton.Visible = project is not null;
		progress.Visible = project is not null;
		if (project is not null)
		{
			progress.Value = 100.0 * project.ElapsedTicks / project.DurationTicks;
			var remaining = Mathf.CeilToInt((project.DurationTicks - project.ElapsedTicks) * TimeTickSystem.TICK_DELTA);
			var hostAlive = player.Entities.TryGetValue(project.StructureId, out var host) && host.Health > 0;
			status.Text = !hostAlive ? "Paused: research host lost"
				: player.EnergyConsumed > player.EnergyProduced ? $"Paused: insufficient energy ({remaining}s remaining)"
				: $"Researching: {remaining}s remaining";
			cancelButton.Text = "Cancel";
		}
		else
		{
			status.Text = completed ? "Research complete."
				: excluded ? doctrinePending ? $"Unavailable while {chosenDoctrine.Name} is researching. Cancel it to reopen this path."
					: $"Permanently excluded by {chosenDoctrine.Name}."
				: busy is not null ? $"{selected.Chapter} is busy with {ResearchCatalog.All.First(item => item.Id == busy.ResearchId).Name}. Select that project to cancel."
				: locked.Length > 0 ? locked
				: ResearchHost(player) is null ? "A Command Center is needed to start research in this outline."
				: player.Virelium < materialCost ? "Not enough Materials."
				: "One active project per chapter. Cancel before completion for a full refund.";
		}
		foreach (var technology in ResearchCatalog.All)
		{
			var card = cards[technology.Id];
			var cardLevel = player.GetResearchLevel(technology.Id);
			var cardCompleted = cardLevel >= technology.MaxLevel;
			var cardResearching = player.ResearchProjects.ContainsKey(technology.Id);
			var cardLocked = !cardCompleted && !cardResearching && ResearchCatalog.LockReason(player, technology).Length > 0;
			if (cardCompleted && completedCards.Add(technology.Id))
			{
				card.AddThemeStyleboxOverride("normal", completedNormal);
				card.AddThemeStyleboxOverride("hover", completedHover);
				card.AddThemeStyleboxOverride("pressed", completedSelected);
				card.AddThemeStyleboxOverride("hover_pressed", completedSelected);
			}
			var otherPath = IsOtherDoctrinePath(technology, chosenDoctrine);
			var state = otherPath ? doctrinePending ? "OTHER PATH PENDING" : "EXCLUDED"
				: cardCompleted ? "COMPLETE"
				: cardResearching ? "RESEARCHING" : "";
			var doctrine = technology.Id is "infantry_doctrine" or "tank_doctrine";
			if (doctrine && chosenDoctrine is null)
				state = technology.Tree == "Infantry" ? "EXCLUDES TANKS" : "EXCLUDES INFANTRY";
			if (technology.MaxLevel > 1)
				state = $"{cardLevel}/{technology.MaxLevel}" + (state.Length > 0 ? $"  |  {state}" : "");
			card.Text = state.Length == 0
				? technology.Name
				: $"{technology.Name}\n{state}";
			card.Modulate = otherPath ? new Color(0.45f, 0.45f, 0.45f)
				: cardLocked ? new Color(0.55f, 0.55f, 0.55f) : Colors.White;
		}
	}

	private static bool IsOtherDoctrinePath(ResearchDefinition technology, ResearchDefinition chosenDoctrine) =>
		chosenDoctrine is not null && technology.Chapter == ResearchChapter.Military && technology.Generation > 1
		&& technology.Tree != "Aircraft" && technology.Tree != chosenDoctrine.Tree;

	private void StartResearch()
	{
		var player = LocalPlayer;
		var host = player is null ? null : ResearchHost(player);
		if (startButton.Disabled || host is null || selected is null)
			return;
		var accepted = simulation.Push(new StartResearchMessage("player_1", simulation.GetState().Tick,
			host.EntityId, selected.Id));
		RefreshProjects();
		if (!accepted)
			status.Text = "Research could not start. Check prerequisites, Materials, and active projects.";
	}

	private void CancelResearch()
	{
		var player = LocalPlayer;
		if (player is null || !player.ResearchProjects.TryGetValue(selected.Id, out var project))
			return;
		simulation.Push(new CancelResearchMessage("player_1", simulation.GetState().Tick, project.StructureId, project.ResearchId));
		RefreshProjects();
	}

	private static Color ChapterColor(ResearchChapter chapter) => chapter switch
	{
		ResearchChapter.Military => new Color("d5b770"),
		ResearchChapter.Civilian => new Color("84b7a0"),
		_ => new Color("8eaec4")
	};

	private static StyleBoxFlat CardStyle(Color background, Color border) => new()
	{
		BgColor = background, BorderColor = border, BorderWidthLeft = 4,
		BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
		CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
		CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
	};

	private partial class ResearchTreeCanvas : Control
	{
		public ResearchChapter Chapter { get; set; }
		public ResearchDefinition ChosenDoctrine { get; set; }
		public Dictionary<string, Vector2> Positions { get; } = new();

		public override void _Draw()
		{
			foreach (var technology in ResearchCatalog.All.Where(technology => technology.Chapter == Chapter))
			{
				foreach (var prerequisite in technology.Prerequisites)
				{
					var end = Positions[technology.Id] + new Vector2(CardSize.X / 2, 0);
					var start = Positions[prerequisite] + new Vector2(CardSize.X / 2, CardSize.Y);
					var color = IsOtherDoctrinePath(technology, ChosenDoctrine) ? new Color("32352f") : new Color("666952");
					if (Positions[technology.Id].Y == Positions[prerequisite].Y)
					{
						start = Positions[prerequisite] + new Vector2(0, CardSize.Y / 2);
						end = Positions[technology.Id] + new Vector2(CardSize.X, CardSize.Y / 2);
						DrawLine(start, end, color, 2, true);
					}
					else
					{
						var middleY = (start.Y + end.Y) / 2;
						DrawPolyline([start, new(start.X, middleY), new(end.X, middleY), end], color, 2, true);
					}
					DrawCircle(end, 4, color.Lightened(0.2f));
				}
			}
		}
	}
}
