using Godot;

public partial class ClientSquadState : Node2D
{
    public string SquadId { get; private set; }
    private Vector2 renderPosition;
    private float movementSpeed;

    public void ApplySpawnState(SquadState state)
    {
        ApplyState(state);
        GlobalPosition = renderPosition;
    }

    /*
    Called on every Tick
    */
    public virtual void ApplyState(SquadState state)
    {
        SquadId = state.SquadId;
        var sum = Vector2.Zero;
        var speedSum = 0f;
        var members = state.Soldiers.Values;
        foreach (var member in members)
        {
            sum += member.CurrentPosition;
            speedSum += member.MovementSpeed;
        }
        var count = state.Soldiers.Count;
        renderPosition = count > 0 ? sum / count : GlobalPosition;
        renderPosition.Y -= 80; // Set height offset for Icon
        movementSpeed = count > 0 ? speedSum / count : 0f;
    }

    public override void _Process(double delta)
    {
        if (GlobalPosition.DistanceTo(renderPosition) <= 3f)
        {
            GlobalPosition = renderPosition;
            return;
        }

        GlobalPosition = GlobalPosition.MoveToward(renderPosition, (float)(movementSpeed * delta));
    }
}
