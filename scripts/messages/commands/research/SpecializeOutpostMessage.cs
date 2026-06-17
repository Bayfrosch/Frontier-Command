using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class SpecializeOutpostMessage : MessageBase, CommandInterface
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface"
    };
    public StringName message_type = GameMessages.SPECIALIZE_OUTPOST;
    public string player_id = "";
    public int issued_at_tick = -1;
    public string outpost_entity_id = "";
    public int specialization = (int)GameMessages.OutpostSpecialization.INDUSTRIAL;
    public SpecializeOutpostMessage(string p_player_id = "", int p_issued_at_tick = -1, string p_outpost_entity_id = "", int p_specialization = (int)GameMessages.OutpostSpecialization.INDUSTRIAL)
    {
        player_id = p_player_id;
        issued_at_tick = p_issued_at_tick;
        outpost_entity_id = p_outpost_entity_id;
        specialization = p_specialization;
    }
    public override GDictionary to_payload() => D(("player_id", player_id), ("issued_at_tick", issued_at_tick), ("outpost_entity_id", outpost_entity_id), ("specialization", specialization));
    public override void from_payload(GDictionary payload)
    {
        player_id = S(payload, "player_id");
        issued_at_tick = I(payload, "issued_at_tick", -1);
        outpost_entity_id = S(payload, "outpost_entity_id");
        specialization = I(payload, "specialization", (int)GameMessages.OutpostSpecialization.INDUSTRIAL);
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(player_id)) Add(errors, "player_id is required.");
        if (issued_at_tick < 0) Add(errors, "issued_at_tick cannot be negative.");
        if (Empty(outpost_entity_id)) Add(errors, "outpost_entity_id is required.");
        if (specialization != (int)GameMessages.OutpostSpecialization.INDUSTRIAL && specialization != (int)GameMessages.OutpostSpecialization.MILITARY && specialization != (int)GameMessages.OutpostSpecialization.RESEARCH) Add(errors, "specialization is invalid.");
        return errors.ToArray();
    }
}
