using System;
using Godot;
public static class BuildingCatalog
{
	public static Vector2 GetFoodprintSize(BuildingType type)
	{
		return type switch
		{
			BuildingType.BARRACKS => new Vector2(80, 60),
			_ => new Vector2(0, 0)
		};
	}
}