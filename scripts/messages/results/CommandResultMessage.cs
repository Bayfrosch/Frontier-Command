using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CommandResultMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    private static readonly StringName[] VALID_STATUSES = {
        "accepted", "rejected"
    };
    private static readonly StringName[] REJECTION_CODES = {
        "malformed_command", "player_not_in_match", "player_defeated", "not_owner", "entity_not_found", "entity_destroyed", "invalid_target", "target_not_visible", "unsupported_command", "invalid_placement", "unreachable_destination", "insufficient_materials", "insufficient_energy", "fleet_capacity_exceeded", "missing_prerequisite", "research_locked", "already_researched", "doctrine_already_chosen", "queue_full", "match_not_running", "rate_limited"
    };
    public StringName message_type = GameMessages.COMMAND_RESULT;
    public string command_id = "";
    public StringName status = default;
    public int accepted_at_tick = -1;
    public GDictionary rejection = new();
    public CommandResultMessage(string p_command_id = "", StringName p_status = default, int p_accepted_at_tick = -1, GDictionary p_rejection = null)
    {
        command_id = p_command_id;
        status = p_status;
        accepted_at_tick = p_accepted_at_tick;
        rejection = p_rejection ?? new GDictionary();
    }
    public override GDictionary to_payload()
    {
        var payload = D(("command_id", command_id), ("status", status));
        if (status == new StringName("accepted")) payload["accepted_at_tick"] = accepted_at_tick;
        else if (status == new StringName("rejected")) payload["rejection"] = rejection;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        command_id = S(payload, "command_id");
        status = SN(payload, "status");
        accepted_at_tick = I(payload, "accepted_at_tick", -1);
        rejection = GD(payload, "rejection");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (Empty(command_id)) Add(errors, "command_id is required.");
        if (!Contains(VALID_STATUSES, status)) Add(errors, "status is invalid.");
        if (status == new StringName("accepted") && accepted_at_tick < 0) Add(errors, "accepted_at_tick cannot be negative.");
        if (status == new StringName("rejected"))
    {
            if (Empty(rejection)) Add(errors, "rejection is required.");
            else if (!Contains(REJECTION_CODES, SN(rejection, "code"))) Add(errors, "rejection.code is invalid.");
        }
        return errors.ToArray();
    }
    private static bool Contains(StringName[] values, StringName value)
    {
        foreach (var v in values) if (v == value) return true;
        return false;
    }
}
