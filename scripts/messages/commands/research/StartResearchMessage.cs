using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class StartResearchMessage : MessageBase, CommandInterface
{
    public StringName message_type = GameMessages.START_RESEARCH;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string research_structure_id = "";
    public string research_id = "";
    public StartResearchMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_research_structure_id = "", string p_research_id = "")
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        research_structure_id = p_research_structure_id;
        research_id = p_research_id;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("research_structure_id", research_structure_id), ("research_id", research_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        research_structure_id = S(payload, "research_structure_id");
        research_id = S(payload, "research_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(research_structure_id)) Add(errors, "research_structure_id is required.");
        if (Empty(research_id)) Add(errors, "research_id is required.");
        return errors.ToArray();
    }
}
