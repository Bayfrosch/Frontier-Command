using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class BuildStructureMessage : MessageBase, CommandInterface, QueueableCommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.BUILD_STRUCTURE;
    public string command_id = "";
    public int issued_at_tick = -1;
    public int queue_mode = (int)GameMessages.QueueMode.REPLACE;
    public string construction_unit_id = "";
    public string building_definition_id = "";
    public Vector2 position = Vector2.Zero;
    public float rotation_radians = 0.0f;
    public BuildStructureMessage(string p_command_id = "", int p_issued_at_tick = -1, string p_construction_unit_id = "", string p_building_definition_id = "", Vector2 p_position = default, float p_rotation_radians = 0.0f, int p_queue_mode = (int)GameMessages.QueueMode.REPLACE)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        construction_unit_id = p_construction_unit_id;
        building_definition_id = p_building_definition_id;
        position = p_position;
        rotation_radians = p_rotation_radians;
        queue_mode = p_queue_mode;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("queue_mode", queue_mode), ("construction_unit_id", construction_unit_id), ("building_definition_id", building_definition_id), ("position", position), ("rotation_radians", rotation_radians));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        queue_mode = I(payload, "queue_mode", (int)GameMessages.QueueMode.REPLACE);
        construction_unit_id = S(payload, "construction_unit_id");
        building_definition_id = S(payload, "building_definition_id");
        position = V2(payload, "position");
        rotation_radians = F(payload, "rotation_radians");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (!QueueModeValid(queue_mode)) Add(errors, "queue_mode is invalid.");
        if (Empty(construction_unit_id)) Add(errors, "construction_unit_id is required.");
        if (Empty(building_definition_id)) Add(errors, "building_definition_id is required.");
        return errors.ToArray();
    }
}
