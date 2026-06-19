using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DebugSpawnUnitsMessage : MessageBase, CommandInterface
{
    public StringName messageType = GameMessages.DEBUG_SPAWN_UNIT;
    public string player_id = "";
    public int issued_at_tick = -1;
    public UnitType unit_type;
    public Vector2 position = Vector2.Zero;
    public DebugSpawnUnitsMessage(
        string p_player_id,
        int p_issued_at_tick,
        UnitType p_unit_type,
        Vector2 p_position
    ) {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        unit_type = p_unit_type;
        position = p_position;
    }
    public override Dictionary to_payload() => D(
        ("player_id", player_id),
        ("issued_at_tick", issued_at_tick),
        ("unit_type", Variant.From(unit_type)),
        ("position", position)
    );
    public override void from_payload(Dictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        position = V2(payload, "position");
        var raw = S(payload, "unit_type");
        if (Enum.TryParse<UnitType>(raw, ignoreCase: true, out var parsed))
            unit_type = parsed;
        else
            unit_type = default;
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(unit_type.ToString())) Add(errors, "unit_type is required.");
        return errors.ToArray();
    }
}
