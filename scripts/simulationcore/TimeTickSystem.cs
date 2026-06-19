using Godot;

public partial class TimeTickSystem : Node
{
    [Signal]
    public delegate void TickEventHandler(int tick);
    private const float TICK_TIMER_MAX = 5;
    private const double TICK_DELTA = 1.0 / TICK_TIMER_MAX;

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
