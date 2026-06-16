using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CancelConstructionMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.CANCEL_CONSTRUCTION;
    public string command_id = "";
    public int issued_at_tick = -1;
    public string construction_site_id = "";
    public CancelConstructionMessage(string p_command_id = "", int p_issued_at_tick = -1, string p_construction_site_id = "")
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        construction_site_id = p_construction_site_id;
    }
    public override GDictionary to_payload() => D(("command_id", command_id), ("issued_at_tick", issued_at_tick), ("construction_site_id", construction_site_id));
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        construction_site_id = S(payload, "construction_site_id");
    }
    public override string[] validate() => Validate3(command_id, issued_at_tick, construction_site_id, "construction_site_id is required.");
    private static string[] Validate3(string command_id, int issued_at_tick, string value, string message)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(command_id)) errors.Add("command_id is required.");
        if (issued_at_tick < 0) errors.Add("issued_at_tick cannot be negative.");
        if (string.IsNullOrEmpty(value)) errors.Add(message);
        return errors.ToArray();
    }
}
