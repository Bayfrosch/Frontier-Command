using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class CancelResearchMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.CANCEL_RESEARCH;
    public string command_id = "";
    public int issued_at_tick = -1;
    public string research_structure_id = "";
    public string research_queue_item_id = "";
    public CancelResearchMessage(string p_command_id = "", int p_issued_at_tick = -1, string p_research_structure_id = "", string p_research_queue_item_id = "")
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        research_structure_id = p_research_structure_id;
        research_queue_item_id = p_research_queue_item_id;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("research_structure_id", research_structure_id), ("research_queue_item_id", research_queue_item_id));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        research_structure_id = S(payload, "research_structure_id");
        research_queue_item_id = S(payload, "research_queue_item_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(research_structure_id)) Add(errors, "research_structure_id is required.");
        if (Empty(research_queue_item_id)) Add(errors, "research_queue_item_id is required.");
        return errors.ToArray();
    }
}
