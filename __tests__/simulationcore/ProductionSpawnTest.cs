using Godot;
using GdUnit4;
using System.Linq;
using static GdUnit4.Assertions;

[TestSuite]
public class ProductionSpawnTest
{
	[TestCase(1)]
	[TestCase(3)]
	public void BatchSpawnsImmediatelyThenEveryTwoTicksWhileNextOrderTrains(int quantity)
	{
		var (context, player, barracks) = CreateProduction(quantity);
		for (var tick = 0; tick < UnitCatalog.GetProductionTime(UnitType.BASIC_INFANTRY) / 5 - 1; tick++)
			context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count()).IsEqual(0);

		context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count()).IsEqual(1);
		AssertThat(barracks.ProductionQueue).IsEmpty();
		AssertThat(context.Push(new TrainUnitsMessage(UnitType.RPG_TROOPER, player.PlayerId, 20, barracks.EntityId))).IsTrue();

		for (var tick = 1; tick <= 6; tick++)
		{
			var existingIds = player.Entities.Values.OfType<UnitState>().Select(unit => unit.EntityId).ToHashSet();
			context.AdvanceTick();
			var units = player.Entities.Values.OfType<UnitState>().ToArray();
			AssertThat(units.Length).IsEqual(System.Math.Min(quantity, 1 + tick / 2));
			AssertThat(barracks.ProductionProgress).IsEqual(tick * 5);
			foreach (var spawned in units.Where(unit => !existingIds.Contains(unit.EntityId)))
				foreach (var existing in units.Where(unit => existingIds.Contains(unit.EntityId)))
					AssertThat(spawned.CurrentPosition.DistanceTo(existing.CurrentPosition) >= 20f).IsTrue();
		}
	}

	[TestCase]
	public void DestroyingBarracksDiscardsRemainingSpawns()
	{
		var (context, player, barracks) = CreateProduction(3);
		for (var tick = 0; tick < UnitCatalog.GetProductionTime(UnitType.BASIC_INFANTRY) / 5; tick++)
			context.AdvanceTick();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count()).IsEqual(1);

		barracks.TakeDamage(barracks.MaxHealth);
		for (var tick = 0; tick < 6; tick++)
			context.AdvanceTick();

		AssertThat(player.Entities.ContainsKey(barracks.EntityId)).IsFalse();
		AssertThat(player.Entities.Values.OfType<UnitState>().Count()).IsEqual(1);
	}

	[TestCase]
	public void SpawnUsesNearbyFreeSpaceWhenExitIsOccupied()
	{
		var (context, player, barracks) = CreateProduction(3);
		var exit = BuildingCatalog.GetFootprintRect(barracks).End + new Vector2(5, 5);
		player.AddEntity(new UnitState("blocker", player.PlayerId, exit, 0f));
		for (var tick = 0; tick < UnitCatalog.GetProductionTime(UnitType.BASIC_INFANTRY) / 5; tick++)
			context.AdvanceTick();

		var spawned = player.Entities.Values.OfType<UnitState>().Single(unit => unit.EntityId != "blocker");
		AssertThat(spawned.CurrentPosition.DistanceTo(exit) >= 20f).IsTrue();
	}

	private static (SimulationContext, PlayerState, BuildingState) CreateProduction(int quantity)
	{
		var context = new SimulationContext("production-test");
		var player = new PlayerState("player");
		context.AddPlayer(player);
		AssertThat(context.Push(new DebugSpawnBuildingMessage(player.PlayerId, 0, BuildingType.BARRACKS, Vector2.Zero))).IsTrue();
		var barracks = player.Entities.Values.OfType<BuildingState>().Single();
		AssertThat(context.Push(new TrainUnitsMessage(UnitType.BASIC_INFANTRY, player.PlayerId, 0, barracks.EntityId, quantity))).IsTrue();
		return (context, player, barracks);
	}
}
