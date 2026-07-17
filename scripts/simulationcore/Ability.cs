using Godot;

/*
Ability data shared by the simulation and client UI.
The Id is used by UseAbilityMessage to route the command.
*/
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
