public sealed class ResearchProjectState
{
	public ResearchProjectState(ResearchDefinition technology, string structureId, int level)
	{
		ResearchId = technology.Id;
		StructureId = structureId;
		Chapter = technology.Chapter;
		Level = level;
		PaidMaterials = technology.GetMaterialCost(level);
		DurationTicks = technology.Seconds * (int)TimeTickSystem.TICK_TIMER_MAX;
	}

	public string ResearchId { get; }
	public int Level { get; }
	public string StructureId { get; }
	public ResearchChapter Chapter { get; }
	public int PaidMaterials { get; }
	public int DurationTicks { get; }
	public int ElapsedTicks { get; private set; }

	internal bool Advance(bool hasEnergy)
	{
		if (hasEnergy)
			ElapsedTicks++;
		return ElapsedTicks >= DurationTicks;
	}
}
