using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class SimulationContext
{
	/*
	Shared constants for simulation pacing and simple formation spacing.
	*/
	const float deltaSeconds = 1.0f / 5.0f;
	private const int CONSTRUCTION_ADVANCE = 5;
	private const int PRODUCTION_ADVANCE = 5;
	private const float UNIT_SPACING = 50f;
	private const float COLLISION_RADIUS = 10f;
	private const float PATH_GRID_SIZE = 30f;
	private const float PATH_SEARCH_MARGIN = 240f;
	public string MatchId { get; }
	/*
	The saved state of the match for each lobby
	can only be edited from SimulationContext, others recieve a readonly
	*/
	private readonly MatchState _matchState = new MatchState();
	/*
	SimulationContext constructor, requires a Match ID
	connecting all message handlers to their specific message type
	*/
	public SimulationContext(string matchId)
	{
		MatchId = matchId;

		// debugging
		Register<DebugSpawnUnitsMessage>(HandleDebugSpawnUnit);
		Register<DebugSpawnBuildingMessage>(HandleDebugSpawnBuilding);

		// construction
		Register<BuildStructureMessage>(msg => HandleBuildStructure(msg));
		Register<CancelConstructionMessage>(HandleCancelConstruction);
		Register<RepairTargetMessage>(HandleRepairTarget);
		Register<UpgradeStructureMessage>(HandleUpgradeStructure);
		Register<CancelStructureUpgradeMessage>(HandleCancelStructureUpgrade);

		// production
		Register<TrainUnitsMessage>(HandleTrainUnits);
		Register<CancelProductionMessage>(HandleCancelProduction);
		Register<SetRallyPointMessage>(HandleSetRallyPoint);

		// research
		Register<StartResearchMessage>(HandleStartResearch);
		Register<CancelResearchMessage>(HandleCancelResearch);
		Register<ChooseCapitalUpgradeMessage>(HandleChooseCapitalUpgrade);
		Register<SpecializeOutpostMessage>(HandleSpecializeOutpost);

		// units 
		Register<MoveUnitsMessage>(HandleMoveUnit);
		Register<AttackMoveUnitsMessage>(HandleAttackMoveUnits);
		Register<AttackTargetMessage>(HandleAttackTarget);
		Register<StopUnitsMessage>(HandleStopUnits);
		Register<HoldPositionMessage>(HandleHoldPosition);
		Register<PatrolUnitsMessage>(HandlePatrolUnits);
		Register<SetUnitStanceMessage>(HandleSetUnitStance);
		Register<UseAbilityMessage>(HandleUseAbility);
		Register<GatherResourcesMessage>(HandleGatherResources);
	}
	/*
	Connects one message type to its handler.
	Push uses this table to route incoming commands.
	*/
	private void Register<TMessage>(Func<TMessage, bool> handler)
		where TMessage : MessageBase
	{
		_handlers[typeof(TMessage)] = msg => handler((TMessage)msg);
	}
	/*
	Processes a Tick by going through all entities
	Advances BuildingState construction by set constant
	*/
	public void AdvanceTick()
	{
		_matchState.IncrementTick();

		var unitsToSpawn = new List<DebugSpawnUnitsMessage>();
		var entitiesToRemove = new List<string>();

		var projectilesToRemove = new List<string>();

		foreach (var projectile in _matchState.Projectiles.Values)
		{
			if (!TryGetPlayerEntity<EntityState>(projectile.TargetPlayerId, projectile.TargetEntityId, out var target)) 
			{
				projectilesToRemove.Add(projectile.ProjectileId);
				continue;
			}
			
			if (projectile.AdvanceMovement(target, TimeTickSystem.TICK_DELTA))
			{
				target.TakeDamage(CalculateDamage(
					projectile.WeaponDefinition.Damage,
					projectile.WeaponDefinition.WeaponClass,
					target
				));
				projectilesToRemove.Add(projectile.ProjectileId);
			}
		}
		
		foreach (var player in _matchState.Players.Values)
		{
			foreach (var entity in player.Entities.Values)
			{
				if (entity is BuildingState building)
				{
					HandleAdvanceConstruction(player.PlayerId, building);
					HandleAdvanceResourceSpawner(building);

					var unit = building.AdvanceProduction(PRODUCTION_ADVANCE);
					if (unit is null)
						continue;

					unitsToSpawn.Add(new DebugSpawnUnitsMessage(
						player.PlayerId,
						_matchState.Tick,
						unit.Value,
						building.RallyPoint,
						UnitCatalog.GetMovementSpeed(unit.Value)
					));
				} else if (entity is UnitState unit)
				{
					unit.AdvanceAttackCooldown(TimeTickSystem.TICK_DELTA);
					HandleAdvanceAttack(unit);
					UpdateMovePathAroundBuildings(unit);
					unit.AdvanceMovement(TimeTickSystem.TICK_DELTA);
					if (unit is ResourceCollectorState collector)
						HandleAdvanceGatherResources(collector);
				}
				
				if (entity.Health <= 0)
				{
					entitiesToRemove.Add(entity.EntityId);
					continue;
				}
			}
		}

		// Remove destroyed entities and projectiles from the match state
		foreach (var entityId in entitiesToRemove)
		{
			foreach (var player in _matchState.Players.Values)
			{
				if (player.Entities.ContainsKey(entityId))
				{
					if (player.Entities[entityId] is BuildingState building
						&& building.Type != BuildingType.CONSTRUCTION_SITE)
					{
						player.RemoveCompletedBuilding(building.Type);
					}
					player.RemoveEntity(entityId);
				}
			}
		}
		foreach (var projectileId in projectilesToRemove)
		{
			_matchState.Projectiles.Remove(projectileId);
		}

		foreach (var spawnMessage in unitsToSpawn)
		{
			Push(spawnMessage);
		}
	}
	/*
	This funtion is for accessing information from the context provider
	Provides a read only Object to get information
	*/
	public IMatchStateView get()
	{
		return _matchState;
	}
	/*
	This function is for accessing what is stored in context provider
	can be accessed with contextProvider.push(...) to store information
	msg Dictionary should consist of pre Defined messages (in scripts/messages)
	 */
	public bool Push(MessageBase msg)
	{
		if (msg is not CommandInterface)
			return false;

		var errors = msg.validate();
		
		if (errors.Length > 0) {
			foreach (var err in errors)
			{
				Console.WriteLine(err);    
			}
			return false;
		}

		if (!_handlers.TryGetValue(msg.GetType(), out var handler))
			return false;

		return handler(msg);
	}
	/*
	Command Handler for directing each message type to the correct handler fuction
	*/
	private readonly Dictionary<System.Type, Func<MessageBase, bool>> _handlers = new();
	/*
	TryGetPlayer gets a playerId as string and gives a PlayerState if found or null
	Returns true if opperation had success and false otherwise
	*/
	private bool TryGetPlayer(string playerId, out PlayerState? player)
	{
		player = null;

		if (string.IsNullOrEmpty(playerId))
			return false;

		return _matchState.Players.TryGetValue(playerId, out player);
	}
	/*
	TryGetPlayerEntity gets a generic which is an EntityState
	It searches for the specific EntityState of the given playerId
	returns true if opperation had success and false otherwise
	*/
	private bool TryGetPlayerEntity<T>(
		string playerId,
		string entityId,
		out T? entity
	) where T : EntityState
	{
		entity = null;

		if (string.IsNullOrEmpty(playerId))
			return false;

		if (!TryGetPlayer(playerId, out var player))
			return false;

		if (player is null)
			return false;

		if (!player.Entities.TryGetValue(entityId, out var rawEntity))
			return false;

		if (rawEntity is not T typedEntity)
			return false;

		entity = typedEntity;

		if (typedEntity is null)
			return false;

		return true;
	}

	/*
	Collects owned units for group commands.
	If one id is invalid the whole command fails.
	*/
	private bool TryGetOwnedUnits(
		string playerId,
		string[] unitIds,
		out List<UnitState>? units
	)
	{
		units = new List<UnitState>();

		foreach (var unitId in unitIds)
		{
			if(!TryGetPlayerEntity<UnitState>(playerId, unitId, out var unit))
			{
				units.Clear();
				return false;
			}
			if (unit is null) 
				return false;
			units.Add(unit);   
		}
		return true;
	}

	/*
	Gets a construction unit owned by the given player.
	Used by building placement and construction assignment.
	*/
	private bool TryGetConstructionUnit(string playerId, string unitId, out UnitState? constructionUnit)
	{
		constructionUnit = null;

		if (!TryGetPlayerEntity<UnitState>(playerId, unitId, out var unit))
			return false;

		if (unit.Type != UnitType.CONSTRUCTION_UNIT)
			return false;

		constructionUnit = unit;
		return true;
	}

	/*
	Finds any entity by id across neutral resources and all player-owned entities.
	Combat can target entities outside the attacking player's dictionary.
	*/
	private bool TryGetEntityById(string entityId, out EntityState? entity)
	{
		entity = _matchState.Resources.Values
			.FirstOrDefault(resource => resource.EntityId == entityId);

		if (entity is not null)
			return true;

		entity = _matchState.Players.Values
			.SelectMany(player => player.Entities.Values)
			.FirstOrDefault(entity => entity.EntityId == entityId);

		return entity is not null;
	}

	/*
	Advances one unit's current attack order.
	The unit moves into range, waits for windup and cooldown, then applies damage.
	*/
	private void HandleAdvanceAttack(UnitState unit)
	{
		if (!unit.HasAttackOrder)
			return;

		if (!TryGetEntityById(unit.AttackTargetId, out var target) || target is null)
		{
			unit.ClearAttackOrder();
			unit.ClearMoveOrder();
			return;
		}

		if (target.OwnerPlayerId == unit.OwnerPlayerId || target.Health <= 0)
		{
			unit.ClearAttackOrder();
			unit.ClearMoveOrder();
			return;
		}

		if (unit.AttackDamage <= 0 || unit.AttackRange <= 0f)
		{
			unit.ClearAttackOrder();
			unit.ClearMoveOrder();
			return;
		}

		if (!CanDamage(unit.WeaponClass, target))
		{
			unit.ClearAttackOrder();
			unit.ClearMoveOrder();
			return;
		}

		var attackRangeSquared = unit.AttackRange * unit.AttackRange;
		if (unit.CurrentPosition.DistanceSquaredTo(target.CurrentPosition) > attackRangeSquared)
		{
			unit.ResetAttackWindup();
			unit.SetMoveOrder(target.CurrentPosition + unit.AttackFormationOffset, preserveAttackOrder: true);
			return;
		}

		unit.ClearMoveOrder();
		if (unit.IsAttackCoolingDown)
		{
			unit.ResetAttackWindup();
			return;
		}

		if (!unit.AdvanceAttackWindup(TimeTickSystem.TICK_DELTA))
			return;

		target.TakeDamage(CalculateDamage(unit.AttackDamage, unit.WeaponClass, target));
		unit.ResetAttackWindup();
		unit.StartAttackCooldown();

		if (target.Health <= 0)
			unit.ClearAttackOrder();
	}

	/*
	Returns true if a weapon can damage the target armor class.
	*/
	private static bool CanDamage(WeaponClass weaponClass, EntityState target)
	{
		return WeaponEffectivenessCatalog.GetModifier(weaponClass, target.ArmorClass) > 0f;
	}

	/*
	Applies the armor modifier and clamps successful hits to at least 1 damage.
	*/
	private static int CalculateDamage(int baseDamage, WeaponClass weaponClass, EntityState target)
	{
		var armorModifier = WeaponEffectivenessCatalog.GetModifier(weaponClass, target.ArmorClass);
		if (armorModifier <= 0f)
			return 0;

		return Math.Max(1, (int)Math.Round(baseDamage * armorModifier, MidpointRounding.AwayFromZero));
	}
	/*
	Message handlers for reacting to every pre defined message in Messages.cs
	Message types get linked in the constructor to their coresponding handler
	All handlers return true or false regarding wether the message was succesfully parsed
	*/
	/*
	Orders selected units to move in a simple rectangular formation.
	*/
	private bool HandleMoveUnit(MoveUnitsMessage msg)
	{
		if (!TryGetOwnedUnits(msg.player_id, msg.unit_ids, out var units) || units is null) {
			GD.Print(units is null);
			return false;
		}

		for (int i = 0; i < units.Count; i++)
		{
			units[i].SetMoveOrder(msg.destination + GetFormationOffset(i, units.Count));
		}

		return true;
	}
	/*
	Creates a construction site and assigns the construction unit that placed it.
	*/
	private bool HandleBuildStructure(BuildStructureMessage msg, int constructionCost = 0)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!TryGetConstructionUnit(msg.player_id, msg.construction_unit_id, out var unit))
			return false;

		var id = NewEntityId(msg.building_type.ToString());
		BuildingState building = new BuildingState(
			id,
			msg.player_id,
			msg.position,
			msg.building_type,
			msg.construction_unit_id,
			constructionCost
		);

		player.AddEntity(building);
		AssignConstructionUnitToSite(unit, building);

		return true;
	}
	/*
	Completed resource spawners create neutral resource nodes once.
	*/
	private void HandleAdvanceResourceSpawner(BuildingState building)
	{
		if (building.Type != BuildingType.RESOURCE_SPAWNER || building.HasSpawnedResources)
			return;

		var resourceType = ResourceCatalog.GetSpawnerResourceType(building.Type);
		var resourceCount = ResourceCatalog.GetSpawnerResourceCount(building.Type);
		var resourceRadius = ResourceCatalog.GetSpawnerResourceRadius(building.Type);
		var maxAmount = ResourceCatalog.GetMaxAmount(resourceType);

		for (var i = 0; i < resourceCount; i++)
		{
			var angle = Math.Tau * i / resourceCount;
			var offset = new Vector2(
				(float)Math.Cos(angle) * resourceRadius,
				(float)Math.Sin(angle) * resourceRadius
			);
			var resource = new ResourceState(
				NewEntityId(resourceType.ToString()),
				resourceType,
				building.EntityId,
				building.CurrentPosition + offset,
				maxAmount
			);

			_matchState.AddResource(resource);
			building.RegisterSpawnedResource(resource.EntityId);
		}

		building.MarkResourcesSpawned();
	}
	/*
	Finds a point just outside the building footprint for construction work.
	*/
	private Vector2 FindShortestPathToBuilding(Vector2 unitPosition, BuildingState building)
	{
		var buildingSize = BuildingCatalog.GetFoodprintSize(BuildingCatalog.GetFootprintType(building));
		var buildingCenter = building.CurrentPosition;
		var direction = unitPosition - buildingCenter;

		if (direction == Vector2.Zero)
			direction = Vector2.Right;

		direction = direction.Normalized();

		var distanceToBuildingEdge = Math.Min(
			Math.Abs(buildingSize.X/2f / direction.X),
			Math.Abs(buildingSize.Y/2f / direction.Y)
		);

		return buildingCenter + direction * (distanceToBuildingEdge + GetConstructionRange(building.Type) / 2);
	}
	/*
	Advances construction only when the assigned unit has arrived in range.
	*/
	private void HandleAdvanceConstruction(string playerId, BuildingState building)
	{
		if (!(building.Type == BuildingType.CONSTRUCTION_SITE))
			return;

		if (!TryGetConstructionUnit(playerId, building.ConstructionUnitId, out var constructionUnit))
			return;

		if (!constructionUnit.HasConstructionOrder || constructionUnit.ConstructionTargetId != building.EntityId)
			return;

		if (constructionUnit.HasMoveOrder)
			return;

		float constructionRange = GetConstructionRange(building.Type);

		if (DistanceSquaredToBuildingFootprint(constructionUnit.CurrentPosition, building) > constructionRange * constructionRange)
			return;

		building.AdvanceConstruction(CONSTRUCTION_ADVANCE);
		if (building.Type != BuildingType.CONSTRUCTION_SITE) {
			if (!TryGetPlayer(playerId, out var player))
				return;
			player.AddCompletedBuilding(building.Type);
			constructionUnit.ClearConstructionOrder();
		}
	}
	/*
	Measures distance to the closest point on a building footprint.
	*/
	private static float DistanceSquaredToBuildingFootprint(Vector2 point, BuildingState building)
	{
		var size = BuildingCatalog.GetFoodprintSize(BuildingCatalog.GetFootprintType(building));
		var halfSize = size / 2f;

		float closestX = Math.Clamp(
			point.X,
			building.CurrentPosition.X - halfSize.X,
			building.CurrentPosition.X + halfSize.X
		);

		float closestY = Math.Clamp(
			point.Y,
			building.CurrentPosition.Y - halfSize.Y,
			building.CurrentPosition.Y + halfSize.Y
		);
		
		return point.DistanceSquaredTo(new Vector2(closestX, closestY));
	}
	/*
	Construction range is currently shared by all construction sites.
	*/
	private static float GetConstructionRange(BuildingType type)
	{
		return 10f;
	}
	/*
	Removes an unfinished construction site.
	*/
	private bool HandleCancelConstruction(CancelConstructionMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!TryGetPlayerEntity<BuildingState>(msg.player_id, msg.construction_site_id, out var building) || building is null)
			return false;
		
		if (building.BuildProgression >= 100)
			return false;

		var removed = player.RemoveEntity(msg.construction_site_id);
		if (removed)
		{
			player.AddMaterials(building.ConstructionCost);
		}

		return removed;
	}
	/*
	Repair commands currently assign construction units to sites or mark damaged entities for repair.
	*/
	private bool HandleRepairTarget(RepairTargetMessage msg)
	{
		if (!TryGetOwnedUnits(msg.player_id, msg.repair_unit_ids, out _))
			return false;

		if (!TryGetPlayerEntity<EntityState>(msg.player_id, msg.target_entity_id, out var entity) || entity is null)
			return false;

		if (entity is BuildingState { Type: BuildingType.CONSTRUCTION_SITE } constructionSite)
			return HandleAssignConstruction(msg.player_id, msg.repair_unit_ids, constructionSite);

		if (entity.Health == entity.MaxHealth)
			return false;

		return entity.GettingRepaired = true;
	}

	/*
	Finds the first valid construction unit and assigns it to the site.
	*/
	private bool HandleAssignConstruction(string playerId, string[] constructionUnitIds, BuildingState constructionSite)
	{
		foreach (var unitId in constructionUnitIds)
		{
			if (!TryGetConstructionUnit(playerId, unitId, out var constructionUnit) || constructionUnit is null)
				continue;

			AssignConstructionUnitToSite(constructionUnit, constructionSite);
			return true;
		}

		return false;
	}

	/*
	Reassigns builder and site state so only one unit owns the construction task.
	*/
	private void AssignConstructionUnitToSite(UnitState constructionUnit, BuildingState constructionSite)
	{
		if (constructionUnit.HasConstructionOrder
		&& constructionUnit.ConstructionTargetId != constructionSite.EntityId
		&& TryGetEntityById(constructionUnit.ConstructionTargetId, out var previousSite)
		&& previousSite is BuildingState previousConstructionSite
		&& previousConstructionSite.ConstructionUnitId == constructionUnit.EntityId)
		{
			previousConstructionSite.ClearConstructionUnit();
		}

		if (!string.IsNullOrEmpty(constructionSite.ConstructionUnitId)
		&& constructionSite.ConstructionUnitId != constructionUnit.EntityId
		&& TryGetPlayerEntity<UnitState>(constructionSite.OwnerPlayerId ?? "", constructionSite.ConstructionUnitId, out var previousConstructionUnit)
		&& previousConstructionUnit is not null
		&& previousConstructionUnit.ConstructionTargetId == constructionSite.EntityId)
		{
			previousConstructionUnit.ClearConstructionOrder();
			previousConstructionUnit.ClearMoveOrder();
		}

		constructionSite.AssignConstructionUnit(constructionUnit.EntityId);
		constructionUnit.SetConstructionOrder(constructionSite.EntityId);
		constructionUnit.SetMoveOrder(
			FindShortestPathToBuilding(constructionUnit.CurrentPosition, constructionSite),
			preserveConstructionOrder: true
		);
	}
	// TODO:
	private bool HandleCaptureTarget(UseAbilityMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleUpgradeStructure(UpgradeStructureMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleCancelStructureUpgrade(CancelStructureUpgradeMessage msg)
	{
		return false;
	}
	/*
	Adds unit production requests to a building queue.
	*/
	private bool HandleTrainUnits(TrainUnitsMessage msg)
	{
		if (!TryGetPlayerEntity<BuildingState>(msg.PlayerId, msg.ProducerEntityId, out var building) || building is null)
			return false;

		for (var i = 0; i < msg.quantity; i++)
			building.QueueProduction(msg.UnitDefinitionId);

		return true;
	}
	/*
	Removes a matching unit type from a building production queue.
	*/
	private bool HandleCancelProduction(CancelProductionMessage msg)
	{
		if (!TryGetPlayerEntity<BuildingState>(msg.PlayerId, msg.ProducerEntityId, out var building))
			return false;

		if (building is null)
			return false;

		if (!building.ProductionQueue.Contains(msg.QueueItem))
			return false;

		building.CancelProduction(msg.QueueItem);
		return true;
	}
	
	/*
	Updates where selected production buildings send finished units.
	*/
	private bool HandleSetRallyPoint(SetRallyPointMessage msg)
	{
		foreach (var entityId in msg.producer_entity_ids)
		{
			if (!TryGetPlayerEntity<BuildingState>(msg.player_id, entityId, out var producer))
				return false;

			producer!.SetRallyPoint(msg.target_position);
		}
		return true;
	}
	// TODO:
	private bool HandleStartResearch(StartResearchMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleCancelResearch(CancelResearchMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleChooseCapitalUpgrade(ChooseCapitalUpgradeMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleSpecializeOutpost(SpecializeOutpostMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleAttackMoveUnits(AttackMoveUnitsMessage msg)
	{
		return false;
	}
	/*
	Orders combat-capable units to attack one target.
	Units that cannot damage the target are ignored.
	*/
	private bool HandleAttackTarget(AttackTargetMessage msg)
	{
		if (!TryGetOwnedUnits(msg.player_id, msg.unit_ids, out var units) || units is null)
			return false;

		if (!TryGetEntityById(msg.target_id, out var target) || target is null)
			return false;

		if (target.OwnerPlayerId == msg.player_id)
			return false;

		var attackOrderAssigned = false;
		var attackers = units
			.Where(unit => unit.AttackDamage > 0
				&& unit.AttackRange > 0f
				&& CanDamage(unit.WeaponClass, target))
			.ToArray();

		for (var i = 0; i < attackers.Length; i++)
		{
			attackers[i].SetAttackOrder(target.EntityId, GetFormationOffset(i, attackers.Length));
			attackOrderAssigned = true;
		}

		return attackOrderAssigned;
	}
	// TODO:
	private bool HandleStopUnits(StopUnitsMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleHoldPosition(HoldPositionMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandlePatrolUnits(PatrolUnitsMessage msg)
	{
		return false;
	}
	// TODO:
	private bool HandleSetUnitStance(SetUnitStanceMessage msg)
	{
		return false;
	}
	/*
	Routes generic ability commands to the concrete simulation handler.
	*/
	private bool HandleUseAbility(UseAbilityMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!player.HasUnlockedAbility(msg.ability_id))
			return false;

		bool passed = false;
		switch (msg.ability_id)
		{
			// Units
			case "capture_building":
				passed = HandleCaptureTarget(msg);
				break;

			// Unit Spawns
			case "spawn_infantry": 
				passed = HandleSpawnUnit(msg, UnitType.BASIC_INFANTRY);
				break;

			case "spawn_rocket_troops":
				passed = HandleSpawnUnit(msg, UnitType.RPG_TROOPER);
				break;

			case "spawn_resource_collector":
				passed = HandleSpawnUnit(msg, UnitType.RESOURCE_COLLECTOR);
				break;

			case "spawn_construction_unit":
				passed = HandleSpawnUnit(msg, UnitType.CONSTRUCTION_UNIT);
				break;

			// Buildings
			case "cancel_construction":
				var cmsg = new CancelConstructionMessage (
					msg.player_id,
					msg.issued_at_tick,
					msg.caster_entity_ids[0]
				);
				passed = HandleCancelConstruction(cmsg);
				break;

			case "sell_building":
				passed = HandleSellBuilding(msg);
				break;

			// Building Spawns
			case "spawn_barracks":
				passed = HandleSpawnBuilding(msg, BuildingType.BARRACKS);
				break;

			case "spawn_resource_gatherer":
				passed = HandleSpawnBuilding(msg, BuildingType.RESOURCE_GATHERER);
				break;

			case "spawn_power_plant":
				passed = HandleSpawnBuilding(msg, BuildingType.POWER_PLANT);
				break;

			case "spawn_war_factory":
				passed = HandleSpawnBuilding(msg, BuildingType.WAR_FACTORY);
				break;
		}
		return passed;
	}

	/*
	Sells completed buildings by removing them from the player's entity list.
	*/
	private bool HandleSellBuilding(UseAbilityMessage msg)
	{
		var soldBuilding = false;
		foreach (var entityId in msg.caster_entity_ids)
		{
			if (!TryGetPlayer(msg.player_id, out var player) || player is null)
				return false;
			
			if (!TryGetPlayerEntity<BuildingState>(msg.player_id, entityId, out var building) || building is null)
				continue;

			if (building.Type == BuildingType.CONSTRUCTION_SITE)
				continue;

			soldBuilding |= player.RemoveEntity(building.EntityId);
			if (soldBuilding)
			{
				player.RemoveCompletedBuilding(building.Type);
				player.AddMaterials(building.ConstructionCost / 2);
			}
		}

		return soldBuilding;
	}

	/*
	Spends the producer building ability cost and queues the requested unit.
	*/
	private bool HandleSpawnUnit(UseAbilityMessage msg, UnitType unitType)
	{
		foreach(string entityId in msg.caster_entity_ids)
		{
			if (!TryGetPlayerEntity<BuildingState>(msg.player_id, entityId, out var building))
				return false;

			if (!TryGetPlayer(msg.player_id, out var player))
				return false;
			
			var abilities = AbilityCatalog.ForBuilding(building.Type);
			var ability = abilities.FirstOrDefault(a => a.Id == msg.ability_id);
			if (ability is null)
			{
				GD.Print("Ability not found");
				return false;
			}
			
			if (player.Virelium < ability.Cost)
			{
				GD.Print("Not enougth Virelium");
				return false;
			}
			
			player.RemoveMaterials(ability.Cost);

			HandleTrainUnits(new TrainUnitsMessage(
				unitType,
				msg.player_id,
				_matchState.Tick,
				building.EntityId,
				1
			));
		}
		return true;
	}

	/*
	Spends the construction unit ability cost and creates a construction site.
	*/
	private bool HandleSpawnBuilding(UseAbilityMessage msg, BuildingType pendingBuilding)
	{
		if (msg.caster_entity_ids.Length > 1)
			return false;

		string entityId = msg.caster_entity_ids[0];

		if (msg.target_position is null)
			return false;

		if (!TryGetPlayer(msg.player_id, out var player))
			return false;
		
		var abilities = AbilityCatalog.ForUnit(UnitType.CONSTRUCTION_UNIT);
		var ability = abilities.FirstOrDefault(a => a.Id == msg.ability_id);
		if (ability is null)
			return false;
		
		if (player.Virelium < ability.Cost)
			return false;
		
		player.RemoveMaterials(ability.Cost);

		var buildStructureMessage = new BuildStructureMessage(
			pendingBuilding,
			msg.player_id,
			_matchState.Tick,
			entityId,
			msg.target_position.Value
		);

		if (HandleBuildStructure(buildStructureMessage, ability.Cost))
			return true;

		player.AddMaterials(ability.Cost);
		return false;
	}

	/*
	Assigns resource collectors to a resource and a valid dropoff building.
	*/
	private bool HandleGatherResources(GatherResourcesMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!_matchState.Resources.TryGetValue(msg.resource_field_entity_id, out var resource) || resource.IsDepleted)
			return false;

		BuildingState? requestedDropoff = null;
		if (!string.IsNullOrEmpty(msg.refinery_entity_id)
			&& !TryGetResourceGatherer(msg.player_id, msg.refinery_entity_id, out requestedDropoff))
		{
			return false;
		}

		var gatherOrderAssigned = false;
		foreach (var collectorId in msg.harvester_entity_ids)
		{
			if (!TryGetPlayerEntity<ResourceCollectorState>(msg.player_id, collectorId, out var collector)
				|| collector is null
				|| collector.Type != UnitType.RESOURCE_COLLECTOR)
			{
				continue;
			}

			var dropoff = requestedDropoff ?? FindNearestResourceGatherer(msg.player_id, collector.CurrentPosition);
			if (dropoff is null)
				continue;

			collector.SetGatherOrder(resource.EntityId, resource.CurrentPosition, dropoff.EntityId);
			gatherOrderAssigned = true;
		}

		return gatherOrderAssigned;
	}

	/*
	Continues the collector gather loop after movement has finished.
	*/
	private void HandleAdvanceGatherResources(ResourceCollectorState collector)
	{
		if (!collector.HasGatherOrder || collector.HasMoveOrder)
			return;

		switch (collector.GatherPhase)
		{
			case ResourceCollectorGatherPhase.MovingToResource:
				HandleCollectorArrivedAtResource(collector);
				break;

			case ResourceCollectorGatherPhase.ReturningToDropoff:
				HandleCollectorArrivedAtDropoff(collector);
				break;
		}
	}

	/*
	Extracts cargo from the resource and sends the collector to the dropoff.
	*/
	private void HandleCollectorArrivedAtResource(ResourceCollectorState collector)
	{
		if (!_matchState.Resources.TryGetValue(collector.ResourceTargetId, out var resource) || resource.IsDepleted)
		{
			collector.ClearGatherOrder();
			return;
		}

		var collectedAmount = resource.Extract(collector.RemainingCapacity);
		collector.Collect(collectedAmount);

		if (!collector.HasCargo)
		{
			collector.ClearGatherOrder();
			return;
		}

		if (!TryGetResourceGatherer(collector.OwnerPlayerId ?? "", collector.ResourceDropoffBuildingId, out var dropoff)
			|| dropoff is null)
		{
			collector.ClearGatherOrder();
			return;
		}

		collector.MoveToDropoff(dropoff.CurrentPosition);
	}

	/*
	Deposits cargo and sends the collector back if the resource still exists.
	*/
	private void HandleCollectorArrivedAtDropoff(ResourceCollectorState collector)
	{
		if (!TryGetPlayer(collector.OwnerPlayerId ?? "", out var player) || player is null)
		{
			collector.ClearGatherOrder();
			return;
		}

		if (!TryGetResourceGatherer(player.PlayerId, collector.ResourceDropoffBuildingId, out var dropoff) || dropoff is null)
		{
			collector.ClearGatherOrder();
			return;
		}

		player.AddMaterials(collector.DepositCargo());

		if (!_matchState.Resources.TryGetValue(collector.ResourceTargetId, out var resource) || resource.IsDepleted)
		{
			collector.ClearGatherOrder();
			return;
		}

		collector.MoveToResource(resource.CurrentPosition);
	}

	/*
	Validates that a building is an owned resource dropoff.
	*/
	private bool TryGetResourceGatherer(string playerId, string buildingId, out BuildingState? resourceGatherer)
	{
		resourceGatherer = null;

		if (!TryGetPlayerEntity<BuildingState>(playerId, buildingId, out var building)
			|| building is null
			|| building.Type != BuildingType.RESOURCE_GATHERER)
		{
			return false;
		}

		resourceGatherer = building;
		return true;
	}

	/*
	Finds the closest owned resource gatherer to use as an automatic dropoff.
	*/
	private BuildingState? FindNearestResourceGatherer(string playerId, Vector2 position)
	{
		if (!TryGetPlayer(playerId, out var player) || player is null)
			return null;

		return player.Entities.Values
			.OfType<BuildingState>()
			.Where(building => building.Type == BuildingType.RESOURCE_GATHERER)
			.OrderBy(building => building.CurrentPosition.DistanceSquaredTo(position))
			.FirstOrDefault();
	}

	/*
	Debug command for spawning units directly into the simulation.
	*/
	private bool HandleDebugSpawnUnit(DebugSpawnUnitsMessage msg)
	{
		if (!TryGetPlayer(msg.PlayerId, out var player))
			return false;

		if (player is null)
			return false;

		var unitId = NewEntityId(msg.UnitType.ToString());
		var spawnPosition = msg.Position;
		var targetPosition = GetOccupiedOffsetPosition(spawnPosition);
		UnitState newUnit = CreateUnitState(unitId, msg.PlayerId, spawnPosition, msg.MovementSpeed, msg.UnitType);

		player.AddEntity(newUnit);

		if (targetPosition != spawnPosition)
			newUnit.SetMoveOrder(targetPosition);

		return true;
	}

	/*
	Creates the correct UnitState subtype for a unit type.
	*/
	private static UnitState CreateUnitState(string entityId, string playerId, Vector2 position, float movementSpeed, UnitType unitType)
	{
		return unitType switch
		{
			UnitType.RESOURCE_COLLECTOR => new ResourceCollectorState(entityId, playerId, position, movementSpeed),
			_ => new UnitState(entityId, playerId, position, movementSpeed, unitType)
		};
	}

	/*
	Debug command for spawning buildings directly into the simulation.
	*/
	private bool HandleDebugSpawnBuilding(DebugSpawnBuildingMessage msg)
	{
		if (!TryGetPlayer(msg.PlayerId, out var player))
			return false;

		if (player is null)
			return false;

		var buildingId = NewEntityId(msg.BuildingType.ToString());
		var building = new BuildingState(
			buildingId,
			msg.PlayerId,
			msg.Position,
			msg.BuildingType,
			msg.ConstructionUnitId
		);

		if (msg.SpawnCompleted)
		{
			building.AdvanceConstruction(100);
			player.AddCompletedBuilding(building.Type);
		}

		player.AddEntity(building);

		if (msg.SpawnCompleted)
			HandleAdvanceResourceSpawner(building);

		return true;
	}

	/*
	Finds nearby free space so debug-spawned units do not stack exactly.
	*/
	private Vector2 GetOccupiedOffsetPosition(Vector2 requestedPosition)
	{
		if (!IsPositionOccupied(requestedPosition))
			return requestedPosition;

		for (var ring = 1; ring <= 8; ring++)
		{
			foreach (var direction in GetSpawnDirectionsForRing(ring))
			{
				var candidate = requestedPosition + direction * UNIT_SPACING;
				if (!IsPositionOccupied(candidate))
					return candidate;
			}
		}

		return requestedPosition + new Vector2(UNIT_SPACING * 9f, 0f);
	}

	/*
	Directions used by GetOccupiedOffsetPosition to search outward in rings.
	*/
	private static IEnumerable<Vector2> GetSpawnDirectionsForRing(int ring)
	{
		yield return new Vector2(ring, 0);
		yield return new Vector2(-ring, 0);
		yield return new Vector2(0, ring);
		yield return new Vector2(0, -ring);
		yield return new Vector2(ring, ring);
		yield return new Vector2(ring, -ring);
		yield return new Vector2(-ring, ring);
		yield return new Vector2(-ring, -ring);
	}

	/*
	Returns a formation offset centered around the destination.
	*/
	private static Vector2 GetFormationOffset(int index, int count)
	{
		if (count <= 1)
			return Vector2.Zero;

		int columns = (int) Math.Ceiling(Math.Sqrt(count));
		int row = index / columns;
		int column = index % columns;

		float offsetX = (column - (columns - 1) / 2f) * UNIT_SPACING;
		int rows = (int) Math.Ceiling(count / (float)columns);
		float offsetY = (row - (rows - 1) / 2f) * UNIT_SPACING;

		return new Vector2(offsetX, offsetY);
	}

	/*
	Adds simple deterministic waypoints around blocking building footprints.
	*/
	private void UpdateMovePathAroundBuildings(UnitState unit)
	{
		if (!unit.HasMoveOrder)
			return;

		if (!PathSegmentBlocked(unit.CurrentPosition, unit.CurrentMoveTarget))
			return;

		var path = FindPathAroundBuildings(unit.CurrentPosition, unit.TargetPosition);
		unit.SetMovePath(path);
	}

	private IReadOnlyList<Vector2> FindPathAroundBuildings(Vector2 start, Vector2 destination)
	{
		if (!PathSegmentBlocked(start, destination))
			return new[] { destination };

		var blockingRects = GetPathBlockingRects().ToArray();
		var startCell = CellFromPoint(start);
		var destinationCell = CellFromPoint(destination);
		var bounds = GetPathSearchBounds(start, destination, blockingRects);
		var pathCells = FindGridPath(startCell, destinationCell, bounds, blockingRects);

		if (pathCells.Count <= 0)
			return new[] { destination };

		var waypoints = pathCells
			.Skip(1)
			.Select(CellCenter)
			.ToList();
		waypoints.Add(destination);
		return SimplifyPath(start, waypoints, blockingRects);
	}

	private bool PathSegmentBlocked(Vector2 start, Vector2 destination)
	{
		return GetPathBlockingRects()
			.Any(rect => SegmentIntersectsRect(start, destination, rect));
	}

	private IEnumerable<Rect2> GetPathBlockingRects()
	{
		return _matchState.Players.Values
			.SelectMany(player => player.Entities.Values)
			.OfType<BuildingState>()
			.Where(building => building.Health > 0)
			.Select(GetPathBlockingRect);
	}

	private static List<Vector2> SimplifyPath(Vector2 start, IReadOnlyList<Vector2> waypoints, IReadOnlyList<Rect2> blockingRects)
	{
		if (waypoints.Count <= 1)
			return waypoints.ToList();

		var simplified = new List<Vector2>();
		var anchor = start;
		var index = 0;

		while (index < waypoints.Count)
		{
			var furthest = index;
			for (var candidate = waypoints.Count - 1; candidate >= index; candidate--)
			{
				if (SegmentClear(anchor, waypoints[candidate], blockingRects))
				{
					furthest = candidate;
					break;
				}
			}

			simplified.Add(waypoints[furthest]);
			anchor = waypoints[furthest];
			index = furthest + 1;
		}

		return simplified;
	}

	private static bool SegmentClear(Vector2 start, Vector2 destination, IReadOnlyList<Rect2> blockingRects)
	{
		return blockingRects.All(rect => !SegmentIntersectsRect(start, destination, rect));
	}

	private List<(int X, int Y)> FindGridPath(
		(int X, int Y) start,
		(int X, int Y) destination,
		Rect2I bounds,
		IReadOnlyList<Rect2> blockingRects)
	{
		var frontier = new PriorityQueue<(int X, int Y), float>();
		var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();
		var costSoFar = new Dictionary<(int X, int Y), float>
		{
			[start] = 0f
		};

		frontier.Enqueue(start, 0f);

		while (frontier.Count > 0)
		{
			var current = frontier.Dequeue();
			if (current == destination)
				return ReconstructPath(start, destination, cameFrom);

			foreach (var next in GetPathNeighbors(current))
			{
				if (!bounds.HasPoint(new Vector2I(next.X, next.Y)))
					continue;

				if (next != destination && CellBlocked(next, blockingRects))
					continue;

				if (!SegmentClear(CellCenter(current), CellCenter(next), blockingRects))
					continue;

				var movementCost = current.X == next.X || current.Y == next.Y
					? 1f
					: 1.414f;
				var newCost = costSoFar[current] + movementCost;
				if (costSoFar.TryGetValue(next, out var existingCost) && newCost >= existingCost)
					continue;

				costSoFar[next] = newCost;
				var priority = newCost + GridDistance(next, destination);
				frontier.Enqueue(next, priority);
				cameFrom[next] = current;
			}
		}

		return new List<(int X, int Y)>();
	}

	private static List<(int X, int Y)> ReconstructPath(
		(int X, int Y) start,
		(int X, int Y) destination,
		IReadOnlyDictionary<(int X, int Y), (int X, int Y)> cameFrom)
	{
		var current = destination;
		var path = new List<(int X, int Y)> { current };

		while (current != start)
		{
			if (!cameFrom.TryGetValue(current, out current))
				return new List<(int X, int Y)>();

			path.Add(current);
		}

		path.Reverse();
		return path;
	}

	private static IEnumerable<(int X, int Y)> GetPathNeighbors((int X, int Y) cell)
	{
		for (var x = -1; x <= 1; x++)
		{
			for (var y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				yield return (cell.X + x, cell.Y + y);
			}
		}
	}

	private static float GridDistance((int X, int Y) from, (int X, int Y) to)
	{
		var dx = Math.Abs(from.X - to.X);
		var dy = Math.Abs(from.Y - to.Y);
		return Math.Max(dx, dy);
	}

	private static bool CellBlocked((int X, int Y) cell, IReadOnlyList<Rect2> blockingRects)
	{
		var center = CellCenter(cell);
		return blockingRects.Any(rect => rect.HasPoint(center));
	}

	private static (int X, int Y) CellFromPoint(Vector2 point)
	{
		return (
			(int)Math.Round(point.X / PATH_GRID_SIZE),
			(int)Math.Round(point.Y / PATH_GRID_SIZE)
		);
	}

	private static Vector2 CellCenter((int X, int Y) cell)
	{
		return new Vector2(cell.X * PATH_GRID_SIZE, cell.Y * PATH_GRID_SIZE);
	}

	private static Rect2I GetPathSearchBounds(Vector2 start, Vector2 destination, IReadOnlyList<Rect2> blockingRects)
	{
		var minX = Math.Min(start.X, destination.X);
		var minY = Math.Min(start.Y, destination.Y);
		var maxX = Math.Max(start.X, destination.X);
		var maxY = Math.Max(start.Y, destination.Y);

		foreach (var rect in blockingRects)
		{
			minX = Math.Min(minX, rect.Position.X);
			minY = Math.Min(minY, rect.Position.Y);
			maxX = Math.Max(maxX, rect.Position.X + rect.Size.X);
			maxY = Math.Max(maxY, rect.Position.Y + rect.Size.Y);
		}

		minX -= PATH_SEARCH_MARGIN;
		minY -= PATH_SEARCH_MARGIN;
		maxX += PATH_SEARCH_MARGIN;
		maxY += PATH_SEARCH_MARGIN;

		var minCell = CellFromPoint(new Vector2(minX, minY));
		var maxCell = CellFromPoint(new Vector2(maxX, maxY));
		return new Rect2I(
			minCell.X,
			minCell.Y,
			maxCell.X - minCell.X + 1,
			maxCell.Y - minCell.Y + 1
		);
	}

	private static Rect2 GetPathBlockingRect(BuildingState building)
	{
		var footprintSize = BuildingCatalog.GetFoodprintSize(BuildingCatalog.GetFootprintType(building));
		return new Rect2(
			building.CurrentPosition - footprintSize / 2f,
			footprintSize
		);
	}

	private static bool SegmentIntersectsRect(Vector2 start, Vector2 end, Rect2 rect)
	{
		if (rect.HasPoint(start))
			return false;

		if (rect.HasPoint(end))
			return true;

		var direction = end - start;
		var tMin = 0f;
		var tMax = 1f;

		return ClipSegmentAxis(start.X, direction.X, rect.Position.X, rect.Position.X + rect.Size.X, ref tMin, ref tMax)
			&& ClipSegmentAxis(start.Y, direction.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y, ref tMin, ref tMax);
	}

	private static bool ClipSegmentAxis(float start, float direction, float min, float max, ref float tMin, ref float tMax)
	{
		if (Math.Abs(direction) < 0.001f)
			return start < min || start > max ? false : true;

		var t1 = (min - start) / direction;
		var t2 = (max - start) / direction;

		if (t1 > t2)
			(t1, t2) = (t2, t1);

		tMin = Math.Max(tMin, t1);
		tMax = Math.Min(tMax, t2);
		return tMin <= tMax;
	}

	/*
	Checks current and target positions to avoid obvious spawn overlap.
	*/
	private bool IsPositionOccupied(Vector2 position)
	{
		var collisionDistanceSquared = COLLISION_RADIUS * COLLISION_RADIUS;

		return _matchState.Players.Values
			.SelectMany(player => player.Entities.Values)
			.OfType<EntityState>()
			.Any(entity =>
				entity.CurrentPosition.DistanceSquaredTo(position) <= collisionDistanceSquared
				|| entity is UnitState unit && unit.HasMoveOrder && unit.TargetPosition.DistanceSquaredTo(position) <= collisionDistanceSquared);
	}

	/*
	Creates unique entity ids with a readable type prefix.
	*/
	private static string NewEntityId(string prefix)
	{
		return $"{prefix}-{Guid.NewGuid():N}";
	}

	/*
	TODO: Setup Method which can later be replaced by a message handler
	*/
	internal void AddPlayer(PlayerState player)
	{
		_matchState.AddPlayer(player);
	}
}

public interface IMatchStateView
{
	/*
	Read-only view exposed to client systems.
	*/
	int Tick { get; }
	IReadOnlyDictionary<string, PlayerState> Players { get; }
	IReadOnlyDictionary<string, ResourceState> Resources { get; }
}
