using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DebugSpawnUnitsMessage : MessageBase, CommandInterface
{
    public StringName messageType = GameMessages.DEBUG_SPAWN_UNIT;
    public string PlayerId = "";
    public int IssuedAtTick = -1;
    public UnitType UnitType;
    public Vector2 Position = Vector2.Zero;
    public float MovementSpeed = 10f;
    public DebugSpawnUnitsMessage(
        string p_player_id,
        int p_issued_at_tick,
        UnitType p_unit_type,
        Vector2 p_position,
        float p_movement_speed
    ) {
        PlayerId = p_player_id;
        IssuedAtTick = p_issued_at_tick;
        UnitType = p_unit_type;
        Position = p_position;
        MovementSpeed = p_movement_speed;
    }
    public override Dictionary to_payload() => D(
        ("player_id", PlayerId),
        ("issued_at_tick", IssuedAtTick),
        ("unit_type", Variant.From(UnitType)),
        ("position", Position),
        ("movement_speed", MovementSpeed)
    );
    public override void from_payload(Dictionary payload)
    {
        PlayerId = S(payload, "player_id");
        IssuedAtTick = I(payload, "issued_at_tick", -1);
        Position = V2(payload, "position");
        MovementSpeed = F(payload, "movement_speed", 10f);
        var raw = S(payload, "unit_type");
        if (Enum.TryParse<UnitType>(raw, ignoreCase: true, out var parsed))
            UnitType = parsed;
        else
            UnitType = default;
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(PlayerId)) Add(errors, "player_id is required.");
        if (IssuedAtTick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(UnitType.ToString())) Add(errors, "unit_type is required.");
        return errors.ToArray();
    }
}
