using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DebugSpawnBuildingMessage : MessageBase, CommandInterface
{
    public StringName messageType = GameMessages.DEBUG_SPAWN_BUILDING;
    public string PlayerId = "";
    public int IssuedAtTick = -1;
    public BuildingType BuildingType;
    public Vector2 Position = Vector2.Zero;
    public bool SpawnCompleted = true;
    public string ConstructionUnitId = "";

    public DebugSpawnBuildingMessage(
        string p_player_id,
        int p_issued_at_tick,
        BuildingType p_building_type,
        Vector2 p_position,
        bool p_spawn_completed = true,
        string p_construction_unit_id = ""
    ) {
        PlayerId = p_player_id;
        IssuedAtTick = p_issued_at_tick;
        BuildingType = p_building_type;
        Position = p_position;
        SpawnCompleted = p_spawn_completed;
        ConstructionUnitId = p_construction_unit_id;
    }

    public override Dictionary to_payload() => D(
        ("player_id", PlayerId),
        ("issued_at_tick", IssuedAtTick),
        ("building_type", Variant.From(BuildingType)),
        ("position", Position),
        ("spawn_completed", SpawnCompleted),
        ("construction_unit_id", ConstructionUnitId)
    );

    public override void from_payload(Dictionary payload)
    {
        PlayerId = S(payload, "player_id");
        IssuedAtTick = I(payload, "issued_at_tick", -1);
        Position = V2(payload, "position");
        SpawnCompleted = B(payload, "spawn_completed", true);
        ConstructionUnitId = S(payload, "construction_unit_id");
        var raw = S(payload, "building_type");
        if (Enum.TryParse<BuildingType>(raw, ignoreCase: true, out var parsed))
            BuildingType = parsed;
        else
            BuildingType = default;
    }

    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(PlayerId)) Add(errors, "player_id is required.");
        if (IssuedAtTick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (BuildingType == BuildingType.CONSTRUCTION_SITE) Add(errors, "building_type must be a completed building type.");
        return errors.ToArray();
    }
}
