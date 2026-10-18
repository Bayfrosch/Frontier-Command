public sealed class ResearchProjectState
{
	public ResearchProjectState(ResearchDefinition technology, string structureId)
	{
		ResearchId = technology.Id;
		StructureId = structureId;
		Chapter = technology.Chapter;
		PaidMaterials = technology.MaterialCost;
		DurationTicks = technology.Seconds * (int)TimeTickSystem.TICK_TIMER_MAX;
	}

	public string ResearchId { get; }
	public string StructureId { get; }
	public ResearchChapter Chapter { get; }
	public int PaidMaterials { get; }
	public int DurationTicks { get; }
	public int ElapsedTicks { get; private set; }

	internal bool Advance() => ++ElapsedTicks >= DurationTicks;
}
