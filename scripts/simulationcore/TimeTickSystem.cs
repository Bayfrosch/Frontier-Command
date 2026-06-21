using Godot;

public partial class TimeTickSystem : Node
{
    [Signal]
    public delegate void TickEventHandler(int tick);
    public const float TICK_TIMER_MAX = 5;
    public const float TICK_DELTA = 1.0f / TICK_TIMER_MAX;

    private double _accumulator;
    private int _currentTick;
    public int CurrentTick => _currentTick;

    public override void _Process(double delta)
    {
        _accumulator += delta;

        while (_accumulator >= TICK_DELTA)
        {
            EmitSignal("Tick", _currentTick);
            _accumulator -= TICK_DELTA;
            _currentTick++;
        }
    }
}
