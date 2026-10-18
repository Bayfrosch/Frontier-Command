using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class ResearchTest
{
	private static (SimulationContext Context, PlayerState Player, BuildingState Host) CreateGame(int materials = 10000)
	{
		var context = new SimulationContext("research-test");
		var player = new PlayerState("player", materials);
		var host = new BuildingState("center", "player", Vector2.Zero, BuildingType.COMMAND_CENTER, "");
		host.AdvanceConstruction(100);
		player.AddEntity(host);
		context.AddPlayer(player);
		return (context, player, host);
	}

	[TestCase]
	public void Start_SpendsOnce_AndAllowsOneProjectPerChapter()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(9000);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "infantry_training"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(9000);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "refining"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(8200);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "fleet_capacity"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(7400);
		AssertThat(player.ResearchProjects.Count).IsEqual(3);
	}

	[TestCase]
	public void Start_RejectsUnknownTechnologyOrHost_WithoutSpending()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "unknown"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "missing", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("missing", 0, "center", "light_tanks"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(10000);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
	}

	[TestCase]
	public void Start_RejectsInsufficientMaterials()
	{
		var (context, player, _) = CreateGame(999);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(999);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
	}

	[TestCase]
	public void Commands_RejectOtherPlayersHostAndProject()
	{
		var (context, player, _) = CreateGame();
		var opponent = new PlayerState("opponent");
		context.AddPlayer(opponent);
		AssertThat(context.Push(new StartResearchMessage("opponent", 0, "center", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"))).IsTrue();
		AssertThat(context.Push(new CancelResearchMessage("opponent", 0, "center", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new CancelResearchMessage("player", 0, "wrong-center", "light_tanks"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(9000);
		AssertThat(opponent.Virelium).IsEqual(20000);
		AssertThat(player.ResearchProjects.Count).IsEqual(1);
	}

	[TestCase]
	public void Cancel_RefundsFullCostAfterProgress_OnlyOnce()
	{
		var (context, player, _) = CreateGame();
		context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"));
		for (var tick = 0; tick < 100; tick++)
			context.AdvanceTick();
		AssertThat(context.Push(new CancelResearchMessage("player", 100, "center", "light_tanks"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(10000);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.Research.ContainsKey("light_tanks")).IsFalse();
		AssertThat(context.Push(new CancelResearchMessage("player", 100, "center", "light_tanks"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(10000);
		AssertThat(context.Push(new StartResearchMessage("player", 100, "center", "light_tanks"))).IsTrue();
	}

	[TestCase]
	public void Complete_UsesSimulationTime_WithoutGrantingGameplayAbilities()
	{
		var (context, player, _) = CreateGame();
		var abilities = player.UnlockedAbilities.ToArray();
		context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"));
		for (var tick = 0; tick < 299; tick++)
			context.AdvanceTick();
		AssertThat(player.Research.ContainsKey("light_tanks")).IsFalse();
		context.AdvanceTick();
		AssertThat(player.Research.ContainsKey("light_tanks")).IsTrue();
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.Virelium).IsEqual(9000);
		AssertThat(player.UnlockedAbilities.SetEquals(abilities)).IsTrue();
		AssertThat(context.Push(new StartResearchMessage("player", 300, "center", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new CancelResearchMessage("player", 300, "center", "light_tanks"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 300, "center", "air_defense"))).IsTrue();
	}

	[TestCase]
	public void LostHost_PausesProject_AndStillAllowsFullRefund()
	{
		var (context, player, host) = CreateGame();
		context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"));
		host.TakeDamage(host.MaxHealth);
		for (var tick = 0; tick < 400; tick++)
			context.AdvanceTick();
		AssertThat(player.Research.ContainsKey("light_tanks")).IsFalse();
		AssertThat(context.Push(new CancelResearchMessage("player", 400, "center", "light_tanks"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(10000);
	}

	private static void CompleteCoreTree(PlayerState player, ResearchChapter chapter, string tree)
	{
		foreach (var technology in ResearchCatalog.All.Where(item => item.Chapter == chapter && item.Tree == tree && item.Generation == 1))
			player.AddResearch(technology.Id);
	}

	[TestCase]
	public void Doctrine_RequiresTwoCompleteMilitaryTreesAndOneCompleteCivilianTree()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "infantry_doctrine"))).IsFalse();
		CompleteCoreTree(player, ResearchChapter.Military, "Infantry");
		CompleteCoreTree(player, ResearchChapter.Civilian, "Extraction");
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "infantry_doctrine"))).IsFalse();
		player.AddResearch("artillery");
		AssertThat(ResearchCatalog.CompletedCoreTrees(player, ResearchChapter.Military)).IsEqual(1);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "infantry_doctrine"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(10000);
		CompleteCoreTree(player, ResearchChapter.Military, "Tanks");
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "infantry_doctrine"))).IsTrue();
		AssertThat(player.Virelium).IsEqual(8000);
	}

	[TestCase]
	public void Doctrine_AcceptsAnyTwoMilitaryTreesAndAnyCivilianTree()
	{
		var militaryPairs = new[] { new[] { "Infantry", "Tanks" }, new[] { "Infantry", "Aircraft" }, new[] { "Tanks", "Aircraft" } };
		foreach (var pair in militaryPairs)
		foreach (var civilian in new[] { "Extraction", "Energy", "Infrastructure" })
		{
			var (context, player, _) = CreateGame();
			foreach (var tree in pair)
				CompleteCoreTree(player, ResearchChapter.Military, tree);
			CompleteCoreTree(player, ResearchChapter.Civilian, civilian);
			AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "tank_doctrine"))).IsTrue();
		}
	}

	[TestCase]
	public void Doctrine_DoesNotCountFleetOrPartialCivilianTrees()
	{
		var (context, player, _) = CreateGame();
		CompleteCoreTree(player, ResearchChapter.Military, "Infantry");
		CompleteCoreTree(player, ResearchChapter.Fleet, "Fleet capacity");
		CompleteCoreTree(player, ResearchChapter.Fleet, "Capital capacity");
		CompleteCoreTree(player, ResearchChapter.Civilian, "Energy");
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "tank_doctrine"))).IsFalse();

		var (otherContext, otherPlayer, _) = CreateGame();
		CompleteCoreTree(otherPlayer, ResearchChapter.Military, "Infantry");
		CompleteCoreTree(otherPlayer, ResearchChapter.Military, "Aircraft");
		otherPlayer.AddResearch("deep_extraction");
		AssertThat(ResearchCatalog.CompletedCoreTrees(otherPlayer, ResearchChapter.Civilian)).IsEqual(0);
		AssertThat(otherContext.Push(new StartResearchMessage("player", 0, "center", "tank_doctrine"))).IsFalse();
	}

	[TestCase]
	public void CompletedDoctrine_ExcludesTheOtherDoctrine()
	{
		foreach (var doctrine in new[] { "infantry_doctrine", "tank_doctrine" })
		{
			var (context, player, _) = CreateGame();
			CompleteCoreTree(player, ResearchChapter.Military, "Infantry");
			CompleteCoreTree(player, ResearchChapter.Military, "Tanks");
			CompleteCoreTree(player, ResearchChapter.Civilian, "Extraction");
			AssertThat(context.Push(new StartResearchMessage("player", 0, "center", doctrine))).IsTrue();
			for (var tick = 0; tick < 600; tick++)
				context.AdvanceTick();
			var balance = player.Virelium;
			var other = doctrine == "infantry_doctrine" ? "tank_doctrine" : "infantry_doctrine";
			AssertThat(context.Push(new StartResearchMessage("player", 600, "center", other))).IsFalse();
			AssertThat(player.Virelium).IsEqual(balance);
		}
	}

	[TestCase]
	public void Research_RequiresEarlierTechnology_AndCannotBuyANewGeneration()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "air_defense"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "next_generation"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(10000);
	}
}
