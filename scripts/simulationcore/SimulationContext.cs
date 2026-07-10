using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class SimulationContext
{
	const float deltaSeconds = 1.0f / 5.0f;
	private const int CONSTRUCTION_ADVANCE = 5;
	private const int PRODUCTION_ADVANCE = 5;
	private const float UNIT_SPACING = 50f;
	private const float COLLISION_RADIUS = 10f;
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

		// construction
		Register<BuildStructureMessage>(HandleBuildStructure);
		Register<CancelConstructionMessage>(HandleCancelConstruction);
		Register<RepairTargetMessage>(HandleRepairTarget);
		Register<CaptureTargetMessage>(HandleCaptureTarget);
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
				target.TakeDamage(projectile.WeaponDefinition.Damage);
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
					HandleAdvanceAttack(unit);
					unit.AdvanceMovement(TimeTickSystem.TICK_DELTA);
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

	private bool TryGetEntityById(string entityId, out EntityState? entity)
	{
		entity = _matchState.Players.Values
			.SelectMany(player => player.Entities.Values)
			.FirstOrDefault(entity => entity.EntityId == entityId);

		return entity is not null;
	}

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

		var attackRangeSquared = unit.AttackRange * unit.AttackRange;
		if (unit.CurrentPosition.DistanceSquaredTo(target.CurrentPosition) > attackRangeSquared)
		{
			unit.ResetAttackWindup();
			unit.SetMoveOrder(target.CurrentPosition + unit.AttackFormationOffset, preserveAttackOrder: true);
			return;
		}

		unit.ClearMoveOrder();
		if (!unit.AdvanceAttackWindup(TimeTickSystem.TICK_DELTA))
			return;

		target.TakeDamage(unit.AttackDamage);
		unit.ResetAttackWindup();

		if (target.Health <= 0)
			unit.ClearAttackOrder();
	}
	/*
	Message handlers for reacting to every pre defined message in Messages.cs
	Message types get linked in the constructor to their coresponding handler
	All handlers return true or false regarding wether the message was succesfully parsed
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
	private bool HandleBuildStructure(BuildStructureMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!TryGetConstructionUnit(msg.player_id, msg.construction_unit_id, out var unit))
			return false;

		var id = NewEntityId(msg.building_type.ToString());
		BuildingState building = new BuildingState(id, msg.player_id, msg.position, msg.building_type, msg.construction_unit_id);

		player.AddEntity(building);
		AssignConstructionUnitToSite(unit, building);

		return true;
	}
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
		if (building.Type != BuildingType.CONSTRUCTION_SITE)
			constructionUnit.ClearConstructionOrder();
	}
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
	private static float GetConstructionRange(BuildingType type)
	{
		return 10f;
	}
	private bool HandleCancelConstruction(CancelConstructionMessage msg)
	{
		if (!TryGetPlayer(msg.player_id, out var player) || player is null)
			return false;

		if (!TryGetPlayerEntity<BuildingState>(msg.player_id, msg.construction_site_id, out var building) || building is null)
			return false;
		
		if (building.BuildProgression >= 100)
			return false;

		return player.RemoveEntity(msg.construction_site_id);
	}
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
	private bool HandleCaptureTarget(CaptureTargetMessage msg)
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
	private bool HandleTrainUnits(TrainUnitsMessage msg)
	{
		if (!TryGetPlayerEntity<BuildingState>(msg.PlayerId, msg.ProducerEntityId, out var building) || building is null)
			return false;

		for (var i = 0; i < msg.quantity; i++)
			building.QueueProduction(msg.UnitDefinitionId);

		return true;
	}
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
	// TODO:
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
			.Where(unit => unit.AttackDamage > 0 && unit.AttackRange > 0f)
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
	// TODO:
	private bool HandleUseAbility(UseAbilityMessage msg)
	{
		bool passed = false;
		switch (msg.ability_id)
		{
			// Units
			case "spawn_infantry": 
				passed = HandleSpawnUnit(msg, UnitType.BASIC_INFANTRY);
				break;
			
			// Buildings
			case "cancel_construction":
				var cmsg = new CancelConstructionMessage (
					msg.player_id,
					msg.issued_at_tick,
					msg.caster_entity_ids[0]
				);
				HandleCancelConstruction(cmsg);
				break;

			case "sell_building":
				passed = HandleSellBuilding(msg);
				break;

			case "spawn_barracks":
				passed = HandleSpawnBuilding(msg, BuildingType.BARRACKS);
				break;
		}
		return passed;
	}

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
		}

		return soldBuilding;
	}

	private bool HandleSpawnUnit(UseAbilityMessage msg, UnitType unitType)
	{
		foreach(string entityId in msg.caster_entity_ids)
		{
			if (!TryGetPlayerEntity<BuildingState>(msg.player_id, entityId, out var building))
				return false;

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

	private bool HandleSpawnBuilding(UseAbilityMessage msg, BuildingType pendingBuilding)
	{
		if (msg.caster_entity_ids.Length > 1)
			return false;

		string entityId = msg.caster_entity_ids[0];

		if (msg.target_position is null)
			return false;

		return HandleBuildStructure(new BuildStructureMessage(
			pendingBuilding,
			msg.player_id,
			_matchState.Tick,
			entityId,
			msg.target_position.Value
		));
	}

	// TODO:
	private bool HandleGatherResources(GatherResourcesMessage msg)
	{
		return false;
	}
	private bool HandleDebugSpawnUnit(DebugSpawnUnitsMessage msg)
	{
		if (!TryGetPlayer(msg.PlayerId, out var player))
			return false;

		if (player is null)
			return false;

		var unitId = NewEntityId(msg.UnitType.ToString());
		var spawnPosition = msg.Position;
		var targetPosition = GetOccupiedOffsetPosition(spawnPosition);
		UnitState newUnit = new UnitState(unitId, msg.PlayerId, spawnPosition, msg.MovementSpeed, msg.UnitType);

		player.AddEntity(newUnit);

		if (targetPosition != spawnPosition)
			newUnit.SetMoveOrder(targetPosition);

		return true;
	}

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
	int Tick { get; }
	IReadOnlyDictionary<string, PlayerState> Players { get; }
}
