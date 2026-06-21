using Godot;
using System;
using System.Reflection.Metadata;

public partial class TestUnit : ClientUnit
{
    public override void _Process(double delta)
    {
        if (Math.Max(GlobalPosition.DistanceTo(RenderPosition), 0) <= 0.8f)
        {
            return; 
        }
        LookAt(TargetPosition);
        GlobalPosition = GlobalPosition.MoveToward(RenderPosition, (float) (MovementSpeed * delta));
    }
}