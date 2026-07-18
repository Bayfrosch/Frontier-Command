using Godot;

public partial class EnergyBar : ProgressBar
{
    private readonly StyleBoxFlat background = new()
    {
        BgColor = NoEnergyBackgroundColor,
        CornerRadiusBottomLeft = 6,
        CornerRadiusBottomRight = 6,
        CornerRadiusTopLeft = 6,
        CornerRadiusTopRight = 6,
    };

    private readonly StyleBoxFlat fill = new()
    {
        BgColor = DrainedEnergyColor,
        CornerRadiusBottomLeft = 6,
        CornerRadiusBottomRight = 6,
        CornerRadiusTopLeft = 6,
        CornerRadiusTopRight = 6,
    };

    private static readonly Color AvailableEnergyColor = new(0.148f, 0.583f, 0.144f);
    private static readonly Color DrainedEnergyColor = new(0.061f, 0.263f, 0.058f);
    private static readonly Color MissingEnergyColor = new(0.72f, 0.1f, 0.08f);
    private static readonly Color NoEnergyBackgroundColor = new(0.194f, 0.194f, 0.194f);

    public override void _Ready()
    {
        AddThemeStyleboxOverride("background", background);
        AddThemeStyleboxOverride("fill", fill);
        ShowPercentage = false;
        ApplyEnergy(0, 0);
    }

    public void ApplyEnergy(int produced, int consumed)
    {
        var clampedProduced = Mathf.Max(0, produced);
        var clampedConsumed = Mathf.Max(0, consumed);
        var missingEnergy = Mathf.Max(0, clampedConsumed - clampedProduced);

        if (missingEnergy > 0)
        {
            MaxValue = Mathf.Max(1, clampedConsumed);
            Value = missingEnergy;
            fill.BgColor = MissingEnergyColor;
        }
        else
        {
            MaxValue = Mathf.Max(1, clampedProduced);
            Value = clampedConsumed;
            fill.BgColor = DrainedEnergyColor;
        }

        background.BgColor = clampedProduced > 0 ? AvailableEnergyColor : NoEnergyBackgroundColor;
    }
}
