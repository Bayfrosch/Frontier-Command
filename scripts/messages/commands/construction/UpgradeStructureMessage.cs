using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class UpgradeStructureMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.UPGRADE_STRUCTURE;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string structure_entity_id = "";
    public string upgrade_definition_id = "";
    public UpgradeStructureMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_structure_entity_id = "", string p_upgrade_definition_id = "")
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        structure_entity_id = p_structure_entity_id;
        upgrade_definition_id = p_upgrade_definition_id;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("structure_entity_id", structure_entity_id), ("upgrade_definition_id", upgrade_definition_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        structure_entity_id = S(payload, "structure_entity_id");
        upgrade_definition_id = S(payload, "upgrade_definition_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(structure_entity_id)) Add(errors, "structure_entity_id is required.");
        if (Empty(upgrade_definition_id)) Add(errors, "upgrade_definition_id is required.");
        return errors.ToArray();
    }
}
