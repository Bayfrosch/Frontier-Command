using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class SetCheatModeMessage : MessageBase, CommandInterface
{
	public StringName message_type = GameMessages.SET_CHEAT_MODE;
	public string PlayerId = "";
	public int IssuedAtTick = -1;
	public bool Enabled;

	public SetCheatModeMessage(string playerId = "", int issuedAtTick = -1, bool enabled = false)
	{
		PlayerId = playerId;
		IssuedAtTick = issuedAtTick;
		Enabled = enabled;
	}

	public override GDictionary to_payload() => D(
		("player_id", PlayerId),
		("issued_at_tick", IssuedAtTick),
		("enabled", Enabled)
	);

	public override void from_payload(GDictionary payload)
	{
		PlayerId = S(payload, "player_id");
		IssuedAtTick = I(payload, "issued_at_tick", -1);
		Enabled = B(payload, "enabled");
	}

	public override string[] validate()
	{
		var errors = new List<string>();
		if (Empty(PlayerId)) Add(errors, "player_id is required.");
		if (IssuedAtTick < 0) Add(errors, "issued_at_tick cannot be negative.");
		return errors.ToArray();
	}
}
