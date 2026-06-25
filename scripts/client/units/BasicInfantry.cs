using Godot;
using System;

public partial class BasicInfantry : ClientUnit
{
    public override void _Process(double delta)
    {
        if (Math.Max(GlobalPosition.DistanceTo(RenderPosition), 0) <= 3f)
        {
            return; 
        }
        LookAt(TargetPosition);
        GlobalPosition = GlobalPosition.MoveToward(RenderPosition, (float) (MovementSpeed * delta));
    }
}