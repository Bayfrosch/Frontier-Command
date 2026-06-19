using Godot;
using System;

public partial class TestUnit : ClientUnit
{
    public override void _Process(double delta)
    {
        GlobalPosition = GlobalPosition.MoveToward(TargetPosition, (float) (MovementSpeed * delta));
        LookAt(TargetPosition);
    }
}