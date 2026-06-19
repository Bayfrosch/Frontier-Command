using Godot;
using System;

public partial class LocalSimulationNode : Node
{
    [Signal]
    public delegate void StateChangedEventHandler();

    public SimulationContext Context { get; private set; } = null!;

    public override void _Ready()
    {
        Context = new SimulationContext("test_match");
        Context.AddPlayer(new PlayerState("player_1"));

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