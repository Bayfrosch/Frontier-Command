using Godot;
using System;

/*
Simulates local game
Emits a Signal on every change with every Tick
Can Push messages to the SimulationCore and therefore change values
This makes it the connection between client and simulation
*/
public partial class LocalSimulationNode : Node
{
    [Signal]
    public delegate void StateChangedEventHandler();

    public SimulationContext Context { get; private set; } = null!;

    public override void _Ready()
    {
        Context = new SimulationContext("test_match");
        Context.AddPlayer(new PlayerState("player_1"));
        Context.AddPlayer(new PlayerState("player_2"));

        var gameLoop = GetNode<TimeTickSystem>("../GameLoop");
        gameLoop.Tick += OnTick;
    }

    public bool Push(MessageBase msg)
    {
        bool accepted = Context.Push(msg);

        if (accepted)
            EmitSignal(SignalName.StateChanged);

        return accepted;
    }

    public IMatchStateView GetState()
    {
        return Context.get();
    }

    private void OnTick(int tick)
    {
        Context.AdvanceTick();
        EmitSignal(SignalName.StateChanged);
    }
}
