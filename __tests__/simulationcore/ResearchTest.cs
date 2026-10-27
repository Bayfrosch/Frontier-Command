using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class ResearchTest
{
	[TestCase]
	public void CommandCenterCannotHostResearch()
	{
		var context = new SimulationContext("research-facility-test");
		var player = new PlayerState("player");
		var center = new BuildingState("command-center", "player", Vector2.Zero, BuildingType.COMMAND_CENTER, "");
		center.AdvanceConstruction(100);
		player.AddEntity(center);
		context.AddPlayer(player);
		var balance = player.Virelium;
		AssertThat(context.Push(new StartResearchMessage("player", 0, center.EntityId, "light_tanks"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(balance);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
	}

	private static (SimulationContext Context, PlayerState Player, BuildingState Host) CreateGame(int materials = 10000)
	{
		var context = new SimulationContext("research-test");
		var player = new PlayerState("player", materials);
		var host = new BuildingState("center", "player", Vector2.Zero, BuildingType.RESEARCH_FACILITY, "");
		host.AdvanceConstruction(100);
		player.AddEntity(host);
		context.AddPlayer(player);
		return (context, player, host);
	}

	[TestCase]
	public void FacilityMustBeCompletedAliveAndOwnedByTheResearchingPlayer()
	{
		var context = new SimulationContext("research-facility-test");
		var player = new PlayerState("player");
		context.AddPlayer(player);
		context.AddPlayer(new PlayerState("opponent"));
		var facility = new BuildingState("lab", "player", Vector2.Zero, BuildingType.RESEARCH_FACILITY, "");
		player.AddEntity(facility);
		var start = new StartResearchMessage("player", 0, facility.EntityId, "light_tanks");
		AssertThat(context.Push(start)).IsFalse();
		AssertThat(player.Virelium).IsEqual(20000);
		facility.AdvanceConstruction(100);
		AssertThat(context.Push(new StartResearchMessage("opponent", 0, facility.EntityId, "light_tanks"))).IsFalse();
		AssertThat(context.Push(start)).IsTrue();
		AssertThat(context.Push(new CancelResearchMessage("player", 0, facility.EntityId, "light_tanks"))).IsTrue();
		facility.TakeDamage(facility.MaxHealth);
		AssertThat(context.Push(start)).IsFalse();
		AssertThat(player.Virelium).IsEqual(20000);
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
	}

	[TestCase]
	public void ConstructionUnitBuildsFacilityBeforeResearchCanStart()
	{
		var context = new SimulationContext("research-facility-test");
		var player = new PlayerState("player");
		context.AddPlayer(player);
		var position = new Vector2(500, 500);
		player.AddEntity(new UnitState("builder", "player", position, 100f, UnitType.CONSTRUCTION_UNIT));
		var ability = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT).Single(item => item.Id == "spawn_research_facility");
		AssertThat(player.UnlockedAbilities.Contains(ability.Id)).IsTrue();
		AssertThat(context.Push(new UseAbilityMessage("player", 0, new[] { "builder" }, ability.Id,
			p_target_position: position))).IsTrue();
		var facility = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(facility.pendingBuilding).IsEqual(BuildingType.RESEARCH_FACILITY);
		AssertThat(player.EnergyConsumed).IsEqual(0);
		AssertThat(player.Virelium).IsEqual(20000 - ability.Cost);
		AssertThat(context.Push(new StartResearchMessage("player", 0, facility.EntityId, "light_tanks"))).IsFalse();
		for (var tick = 0; tick < 100 && facility.Type == BuildingType.CONSTRUCTION_SITE; tick++)
			context.AdvanceTick();
		AssertThat(facility.Type).IsEqual(BuildingType.RESEARCH_FACILITY);
		AssertThat(facility.Health).IsEqual(BuildingCatalog.GetMaxHealth(BuildingType.RESEARCH_FACILITY));
		AssertThat(player.EnergyConsumed).IsEqual(15);
		AssertThat(context.Push(new StartResearchMessage("player", context.get().Tick, facility.EntityId, "light_tanks"))).IsTrue();
	}

	[TestCase]
	public void InfantryBranches_UnlockIndependentlyAfterRecruitment()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "better_weapons"))).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "support_equipment"))).IsFalse();
		player.AddResearch("infantry_training");
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "better_weapons"))).IsTrue();
		AssertThat(context.Push(new CancelResearchMessage("player", 0, "center", "better_weapons"))).IsTrue();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "anti_armor_infantry"))).IsTrue();
		AssertThat(context.Push(new CancelResearchMessage("player", 0, "center", "anti_armor_infantry"))).IsTrue();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "support_equipment"))).IsFalse();
		player.AddResearch("anti_armor_infantry");
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "support_equipment"))).IsTrue();
	}

	[TestCase]
	public void SupportEquipment_TwoLevelsPreserveDamageAndRefundThePriceActuallyPaid()
	{
		var (context, player, _) = CreateGame();
		player.AddResearch("infantry_training");
		player.AddResearch("anti_armor_infantry");
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		var infantry = new UnitState("infantry", "player", new Vector2(500, 0), 100f, UnitType.BASIC_INFANTRY);
		var rpg = new UnitState("rpg", "player", new Vector2(600, 0), 80f, UnitType.RPG_TROOPER);
		var tank = new UnitState("tank", "player", new Vector2(700, 0), 70f, UnitType.LIGHT_TANK);
		player.AddEntity(infantry);
		player.AddEntity(rpg);
		player.AddEntity(tank);
		infantry.TakeDamage(60);
		var opponent = new PlayerState("opponent");
		var enemy = new UnitState("enemy", "opponent", new Vector2(800, 0), 100f, UnitType.BASIC_INFANTRY);
		opponent.AddEntity(enemy);
		context.AddPlayer(opponent);

		for (var level = 1; level <= 2; level++)
		{
			var price = level == 1 ? 1400 : 1800;
			var balance = player.Virelium;
			var start = new StartResearchMessage("player", context.get().Tick, "center", "support_equipment");
			AssertThat(context.Push(start)).IsTrue();
			AssertThat(context.Push(start)).IsFalse();
			AssertThat(player.Virelium).IsEqual(balance - price);
			AssertThat(player.EnergyConsumed).IsEqual(10);
			context.AdvanceTick();
			AssertThat(context.Push(new CancelResearchMessage("player", context.get().Tick, "center", "support_equipment"))).IsTrue();
			AssertThat(player.Virelium).IsEqual(balance);
			AssertThat(player.GetResearchLevel("support_equipment")).IsEqual(level - 1);
			AssertThat(player.EnergyConsumed).IsEqual(0);
			AssertThat(context.Push(start)).IsTrue();
			var project = player.ResearchProjects["support_equipment"];
			AssertThat(project.Level).IsEqual(level);
			AssertThat(project.DurationTicks).IsEqual(375);
			player.RemoveCompletedBuilding(BuildingType.POWER_PLANT);
			context.AdvanceTick();
			AssertThat(project.ElapsedTicks).IsEqual(0);
			player.AddCompletedBuilding(BuildingType.POWER_PLANT);
			for (var tick = 0; tick < project.DurationTicks - 1; tick++)
				context.AdvanceTick();
			AssertThat(infantry.Health).IsEqual(40 + (level - 1) * 25);
			context.AdvanceTick();
			AssertThat(player.GetResearchLevel("support_equipment")).IsEqual(level);
			AssertThat(infantry.Health).IsEqual(40 + level * 25);
			AssertThat(infantry.MaxHealth).IsEqual(100 + level * 25);
			AssertThat(rpg.Health).IsEqual(100 + level * 25);
			AssertThat(rpg.MaxHealth).IsEqual(100 + level * 25);
			AssertThat(tank.MaxHealth).IsEqual(450);
			AssertThat(enemy.MaxHealth).IsEqual(100);
			AssertThat(player.EnergyConsumed).IsEqual(0);
			player.AddResearch("support_equipment", level);
			AssertThat(infantry.Health).IsEqual(40 + level * 25);
		}
		var finalBalance = player.Virelium;
		AssertThat(context.Push(new StartResearchMessage("player", context.get().Tick, "center", "support_equipment"))).IsFalse();
		AssertThat(player.Virelium).IsEqual(finalBalance);
		foreach (var type in new[] { UnitType.BASIC_INFANTRY, UnitType.RPG_TROOPER })
		{
			AssertThat(context.Push(new DebugSpawnUnitsMessage("player", context.get().Tick, type, new Vector2(900, 0), new Vector2(900, 0), 100f))).IsTrue();
			AssertThat(player.Entities.Values.OfType<UnitState>().Where(unit => unit.Type == type).All(unit => unit.MaxHealth == 150)).IsTrue();
		}
		infantry.TakeDamage(1000);
		player.AddResearch("support_equipment", 2);
		AssertThat(infantry.Health).IsEqual(0);
	}

	[TestCase]
	public void SupportEquipment_FirstLevelCountsButDoesNotCompleteOtherResearch()
	{
		var (_, player, _) = CreateGame();
		player.AddResearch("infantry_training");
		player.AddResearch("anti_armor_infantry");
		player.AddResearch("support_equipment", 2);
		AssertThat(ResearchCatalog.CompletedCoreTrees(player, ResearchChapter.Military)).IsEqual(0);
		AssertThat(player.Research.ContainsKey("better_weapons")).IsFalse();

		var (_, other, _) = CreateGame();
		other.AddResearch("infantry_training");
		other.AddResearch("anti_armor_infantry");
		other.AddResearch("better_weapons");
		AssertThat(ResearchCatalog.CompletedCoreTrees(other, ResearchChapter.Military)).IsEqual(0);
		other.AddResearch("support_equipment");
		AssertThat(ResearchCatalog.CompletedCoreTrees(other, ResearchChapter.Military)).IsEqual(1);
		var followingResearch = new ResearchDefinition("following", "Following research", ResearchChapter.Military,
			"Infantry", 1, 100, 0, 1, "", ["support_equipment"]);
		AssertThat(ResearchCatalog.LockReason(other, followingResearch)).IsEqual("");
		AssertThat(other.Research.ContainsKey("following")).IsFalse();
	}

	[TestCase]
	public void AntiArmorSquads_UnlocksRpgProductionOnlyAfterCompletionForItsOwner()
	{
		var (context, player, _) = CreateGame();
		var opponent = new PlayerState("opponent");
		context.AddPlayer(opponent);
		player.AddResearch("infantry_training");
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		AssertThat(context.Push(new DebugSpawnBuildingMessage("player", 0, BuildingType.BARRACKS, new Vector2(500, 0)))).IsTrue();
		var barracks = player.Entities.Values.OfType<BuildingState>().Single(building => building.Type == BuildingType.BARRACKS);
		var ability = new UseAbilityMessage("player", 0, new[] { barracks.EntityId }, "spawn_rocket_troops");
		var train = new TrainUnitsMessage(UnitType.RPG_TROOPER, "player", 0, barracks.EntityId);
		var materials = player.Virelium;
		AssertThat(player.UnlockedAbilities.Contains("spawn_rocket_troops")).IsFalse();
		AssertThat(context.Push(ability)).IsFalse();
		AssertThat(context.Push(train)).IsFalse();
		AssertThat(player.Virelium).IsEqual(materials);
		AssertThat(barracks.ProductionQueue).IsEmpty();

		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "anti_armor_infantry"))).IsTrue();
		context.AdvanceTick();
		AssertThat(context.Push(ability)).IsFalse();
		AssertThat(context.Push(train)).IsFalse();
		AssertThat(context.Push(new CancelResearchMessage("player", 1, "center", "anti_armor_infantry"))).IsTrue();
		AssertThat(context.Push(train)).IsFalse();
		AssertThat(context.Push(new StartResearchMessage("player", 1, "center", "anti_armor_infantry"))).IsTrue();
		var duration = player.ResearchProjects["anti_armor_infantry"].DurationTicks;
		for (var tick = 0; tick < duration - 1; tick++)
			context.AdvanceTick();
		AssertThat(context.Push(train)).IsFalse();
		var revision = player.AbilityUnlockRevision;
		context.AdvanceTick();

		AssertThat(player.AbilityUnlockRevision).IsEqual(revision + 1);
		AssertThat(opponent.UnlockedAbilities.Contains("spawn_rocket_troops")).IsFalse();
		AssertThat(context.Push(ability)).IsTrue();
		AssertThat(context.Push(train)).IsTrue();
		AssertThat(barracks.ProductionQueue.Count).IsEqual(2);
		for (var tick = 0; tick < UnitCatalog.GetProductionTime(UnitType.RPG_TROOPER) / 5; tick++)
			context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Single().Type).IsEqual(UnitType.RPG_TROOPER);
	}

	[TestCase]
	public void BetterWeapons_UpgradesExistingAndNewBasicInfantryAndTheirCombatDamageOnly()
	{
		var (context, player, _) = CreateGame();
		player.AddResearch("infantry_training");
		player.AddResearch("anti_armor_infantry");
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		foreach (var type in System.Enum.GetValues<UnitType>())
			player.AddEntity(new UnitState(type.ToString(), "player", new Vector2(500, 0), 100f, type));
		var infantry = (UnitState)player.Entities[UnitType.BASIC_INFANTRY.ToString()];
		var opponent = new PlayerState("opponent");
		var target = new UnitState("target", "opponent", new Vector2(550, 0), 100f, UnitType.BASIC_INFANTRY);
		opponent.AddEntity(target);
		context.AddPlayer(opponent);
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "better_weapons"))).IsTrue();
		context.AdvanceTick();
		AssertThat(infantry.AttackDamage).IsEqual(10);
		AssertThat(context.Push(new CancelResearchMessage("player", 1, "center", "better_weapons"))).IsTrue();
		AssertThat(infantry.AttackDamage).IsEqual(10);
		AssertThat(context.Push(new StartResearchMessage("player", 1, "center", "better_weapons"))).IsTrue();
		var duration = player.ResearchProjects["better_weapons"].DurationTicks;
		for (var tick = 0; tick < duration - 1; tick++)
			context.AdvanceTick();
		AssertThat(infantry.AttackDamage).IsEqual(10);
		context.AdvanceTick();

		AssertThat(infantry.AttackDamage).IsEqual(12);
		AssertThat(target.AttackDamage).IsEqual(10);
		foreach (var unit in player.Entities.Values.OfType<UnitState>().Where(unit => unit.Type != UnitType.BASIC_INFANTRY))
			AssertThat(unit.AttackDamage).IsEqual(UnitCatalog.GetAttackDamage(unit.Type));
		player.AddResearch("better_weapons");
		AssertThat(infantry.AttackDamage).IsEqual(12);
		AssertThat(context.Push(new DebugSpawnUnitsMessage("player", 400, UnitType.BASIC_INFANTRY,
			new Vector2(800, 0), new Vector2(800, 0), 100f))).IsTrue();
		AssertThat(player.Entities.Values.OfType<UnitState>().Where(unit => unit.Type == UnitType.BASIC_INFANTRY)
			.All(unit => unit.AttackDamage == 12)).IsTrue();

		AssertThat(context.Push(new AttackTargetMessage("player", 400, new[] { infantry.EntityId }, target.EntityId))).IsTrue();
		context.AdvanceTick();
		context.AdvanceTick();
		// Small arms deal 125% damage against light armor: 12 * 1.25 = 15.
		AssertThat(target.Health).IsEqual(target.MaxHealth - 15);
	}

	[TestCase]
	public void PowerDeficit_PausesProgress_AndRestoredPowerResumesUntilCompletion()
	{
		var (context, player, _) = CreateGame();
		AssertThat(context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"))).IsTrue();
		var project = player.ResearchProjects["light_tanks"];
		for (var tick = 0; tick < 10; tick++)
			context.AdvanceTick();
		AssertThat(project.ElapsedTicks).IsEqual(0);
		AssertThat(player.EnergyConsumed).IsEqual(10);

		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		for (var tick = 0; tick < 100; tick++)
			context.AdvanceTick();
		AssertThat(project.ElapsedTicks).IsEqual(100);

		player.RemoveCompletedBuilding(BuildingType.POWER_PLANT);
		for (var tick = 0; tick < 400; tick++)
			context.AdvanceTick();
		AssertThat(project.ElapsedTicks).IsEqual(100);
		AssertThat(player.Research.ContainsKey("light_tanks")).IsFalse();
		AssertThat(player.EnergyConsumed).IsEqual(10);

		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		for (var tick = 100; tick < project.DurationTicks; tick++)
			context.AdvanceTick();
		AssertThat(player.Research.ContainsKey("light_tanks")).IsTrue();
		AssertThat(player.ResearchProjects.Count).IsEqual(0);
		AssertThat(player.EnergyConsumed).IsEqual(0);
		context.AdvanceTick();
		AssertThat(player.EnergyConsumed).IsEqual(0);
	}

	[TestCase]
	public void CancelDuringPowerDeficit_ReleasesOnlyCancelledProjectsEnergy_AndResumesOtherProject()
	{
		var (context, player, _) = CreateGame();
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
		context.Push(new StartResearchMessage("player", 0, "center", "light_tanks"));
		context.Push(new StartResearchMessage("player", 0, "center", "refining"));
		context.AdvanceTick();
		AssertThat(player.ResearchProjects["light_tanks"].ElapsedTicks).IsEqual(0);
		AssertThat(player.ResearchProjects["refining"].ElapsedTicks).IsEqual(0);
		AssertThat(player.EnergyConsumed).IsEqual(15);

		AssertThat(context.Push(new CancelResearchMessage("player", 1, "center", "refining"))).IsTrue();
		AssertThat(player.EnergyConsumed).IsEqual(10);
		AssertThat(context.Push(new CancelResearchMessage("player", 1, "center", "refining"))).IsFalse();
		AssertThat(player.EnergyConsumed).IsEqual(10);
		context.AdvanceTick();
		AssertThat(player.ResearchProjects["light_tanks"].ElapsedTicks).IsEqual(1);
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
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
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
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
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
		player.AddCompletedBuilding(BuildingType.POWER_PLANT);
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
			player.AddCompletedBuilding(BuildingType.POWER_PLANT);
			player.AddCompletedBuilding(BuildingType.POWER_PLANT);
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
