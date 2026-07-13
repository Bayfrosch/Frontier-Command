using Godot;

public class Ability
{
    public string Id;
    public string Name;
    public int Cost;
    public bool RequiresTarget;
    public Image? Preview;
    public bool Unlocked;
    public Vector2? TargetPosition;
}
