using System.Collections.Generic;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DebugSpawnUnitsMessage : MessageBase
{
    public StringName messageType = GameMessages.DEBUG_SPAWN_UNIT;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string unit_definition_id = "";
    public Vector2 position = Vector2.Zero;
    public DebugSpawnUnitsMessage(
        string p_player_id,
        string p_unit_definition_id,
        Vector2 p_position
    ) {
        player_id = p_player_id;
        unit_definition_id = p_unit_definition_id;
        position = p_position;
    }
    public override Dictionary to_payload() => D(
        ("player_id", player_id),
        ("issued_at_tick", issued_at_tick),
        ("unit_definition_id", unit_definition_id),
        ("position", position)
    );
    public override void from_payload(Dictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        unit_definition_id = S(payload, "unit_definition_id");
        position = V2(payload, "position");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(unit_definition_id)) Add(errors, "unit_definition_id is required.");
        return errors.ToArray();
    }
}