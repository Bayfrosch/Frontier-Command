using Godot;

public partial class ResourceCollector : ClientUnit
{
	private ProgressBar CarryBar = null;

	public override void _Ready()
	{
		base._Ready();

		CarryBar = GetNode<ProgressBar>("CarryBar");
		CarryBar.MinValue = 0;
		CarryBar.MaxValue = 1;
		CarryBar.Value = 0;
	}

	public override void ApplyState(UnitState state)
	{
		base.ApplyState(state);

		if (state is not ResourceCollectorState collectorState)
		{
			CarryBar.MaxValue = 1;
			CarryBar.Value = 0;
			return;
		}

		CarryBar.MaxValue = collectorState.MaxCapacity;
		CarryBar.Value = collectorState.Carry;
	}
}
