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
    public string player_id = "";
    public int issued_at_tick = -1;
    public string construction_site_id = "";
    public CancelConstructionMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_construction_site_id = "")
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        construction_site_id = p_construction_site_id;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("construction_site_id", construction_site_id));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        construction_site_id = S(payload, "construction_site_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) errors.Add("issued_at_tick cannot be negative.");
        if (string.IsNullOrEmpty(construction_site_id)) errors.Add("construction_site_id is required.");
        return errors.ToArray();
    }
}
