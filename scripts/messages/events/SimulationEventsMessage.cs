using Godot;
using System.Collections.Generic;
using GDictionary = Godot.Collections.Dictionary;
using GArrayDictionary = Godot.Collections.Array<Godot.Collections.Dictionary>;

[GlobalClass]
public partial class SimulationEventsMessage : MessageBase
{
    public StringName message_type = GameMessages.SIMULATION_EVENTS;
    public int tick = -1;
    public GArrayDictionary events = new();
    public SimulationEventsMessage(int p_tick = -1, GArrayDictionary p_events = null)
    {
        tick = p_tick;
        events = p_events ?? new GArrayDictionary();
    }
    public override GDictionary to_payload() => D(("tick", tick), ("events", events));
    public override void from_payload(GDictionary payload)
    {
        tick = I(payload, "tick", -1);
        events = GAD(payload, "events");
    }
    public override string[] validate()
    {
        var errors = new List<string>();
        if (tick < 0) Add(errors, "tick cannot be negative.");
        foreach (var ev in events)
    {
            if (!ev.ContainsKey("event_id") || string.IsNullOrEmpty(ev["event_id"].AsString())) Add(errors, "event_id is required.");
            if (!ev.ContainsKey("event_type") || ev["event_type"].AsString() == "") Add(errors, "event_type is required.");
            if (!ev.ContainsKey("tick") || ev["tick"].AsInt32() < 0) Add(errors, "event.tick cannot be negative.");
        }
        return errors.ToArray();
    }
}
