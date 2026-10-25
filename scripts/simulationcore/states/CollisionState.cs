public interface ICollisionState
{
	public BuildingType Type { get; }

	public BuildingType pendingBuilding { get; }
}