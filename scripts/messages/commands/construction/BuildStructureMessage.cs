using Godot;
using System;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class BuildStructureMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public StringName message_type = GameMessages.BUILD_STRUCTURE;
    public string player_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string construction_unit_id = "";
    public BuildingType building_type;
    public Vector2 position = Vector2.Zero;
    public float rotation_radians = 0.0f;
    public BuildStructureMessage(
        BuildingType p_building_type, 
        string p_player_id = "", 
        int p_issued_at_tick = -1, 
        string p_construction_unit_id = "", 
        Vector2 p_position = default, 
        float p_rotation_radians = 0.0f, 
        int p_queue_mode = (int)GameMessages.QueueMode.REPLACE
    ) {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        construction_unit_id = p_construction_unit_id;
        building_type = p_building_type;
        position = p_position;
        rotation_radians = p_rotation_radians;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(
        ("player_id", player_id), 
        ("issued_at_tick", issued_at_tick), 
        ("queue_mode", queue_mode), 
        ("construction_unit_id", construction_unit_id), 
        ("building_type", Variant.From(building_type.ToString())), 
        ("position", position), 
        ("rotation_radians", rotation_radians));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        construction_unit_id = S(payload, "construction_unit_id");
        var raw = S(payload, "building_type");
        position = V2(payload, "position");
        rotation_radians = F(payload, "rotation_radians");

        if (Enum.TryParse<BuildingType>(raw, ignoreCase: true, out var parsed))
            building_type = parsed;
        else
            building_type = default;
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(construction_unit_id)) Add(errors, "construction_unit_id is required.");
        if (Empty(building_type.ToString())) Add(errors, "building_type is required.");
        return errors.ToArray();
    }
}
