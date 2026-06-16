using Godot;
using GDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class MoveUnitsMessage : UnitDestinationCommandBase
{
    public static readonly StringName[] IMPLEMENTS = {
        "MessageInterface", "CommandInterface", "QueueableCommandInterface"
    };
    public StringName message_type = GameMessages.MOVE_UNITS;
    public MoveUnitsMessage(string p_command_id = "", int p_issued_at_tick = -1, string[] p_unit_ids = default, Vector2 p_destination = default, int p_queue_mode = (int)GameMessages.QueueMode.REPLACE, StringName p_formation = default)
    {
        command_id = p_command_id;
        issued_at_tick = p_issued_at_tick;
        unit_ids = p_unit_ids ?? System.Array.Empty<string>();
        destination = p_destination;
        queue_mode = p_queue_mode;
        formation = Empty(p_formation) ? new StringName("rectangle"):  p_formation;
    }
    public override GDictionary to_payload() => UnitDestinationPayload();
    public override void from_payload(GDictionary payload) => UnitDestinationFromPayload(payload);
}
