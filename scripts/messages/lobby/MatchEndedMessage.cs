using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class MatchEndedMessage : MessageBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface"
    };
    private static readonly StringName[] VALID_REASONS = {
        "victory_conditions", "surrender", "all_opponents_disconnected", "admin_ended"
    };
    public StringName message_type = GameMessages.MATCH_ENDED;
    public int end_tick = -1;
    public string[] winner_player_ids = System.Array.Empty<string>();
    public string[] defeated_player_ids = System.Array.Empty<string>();
    public StringName reason = default;
    public string winner_team_id = "";
    public MatchEndedMessage(int p_end_tick = -1, string[] p_winner_player_ids = default, string[] p_defeated_player_ids = default, StringName p_reason = default, string p_winner_team_id = "")
    {
        end_tick = p_end_tick;
        winner_player_ids = p_winner_player_ids ?? System.Array.Empty<string>();
        defeated_player_ids = p_defeated_player_ids ?? System.Array.Empty<string>();
        reason = p_reason;
        winner_team_id = p_winner_team_id;
    }
    public override GDictionary to_payload()
    {
        var payload = D(("end_tick", end_tick), ("winner_player_ids", winner_player_ids), ("defeated_player_ids", defeated_player_ids), ("reason", reason));
        if (!Empty(winner_team_id)) payload["winner_team_id"] = winner_team_id;
        return payload;
    }
    public override void from_payload(GDictionary payload)
    {
        end_tick = I(payload, "end_tick", -1);
        winner_player_ids = PSA(payload, "winner_player_ids");
        defeated_player_ids = PSA(payload, "defeated_player_ids");
        reason = SN(payload, "reason");
        winner_team_id = S(payload, "winner_team_id");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (end_tick < 0) Add(errors, "end_tick cannot be negative.");
        if (!Contains(VALID_REASONS, reason)) Add(errors, "reason is invalid.");
        return errors.ToArray();
    }
    private static bool Contains(StringName[] values, StringName value)
    {
        foreach (var v in values) if (v == value) return true;
        return false;
    }
}
