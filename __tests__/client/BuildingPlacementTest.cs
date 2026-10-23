using Godot;
using GdUnit4;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BuildingPlacementTest
{
	[TestCase]
	public async Task Preview_TurnsRedOverNeutralResource_AndClearsWhenDepleted()
	{
		using var runner = ISceneRunner.Load("res://scenes/gameScene.tscn", true, false);
		await runner.SimulateFrames(2);
		var scene = runner.Scene();
		var simulation = scene.GetNode<LocalSimulationNode>("SimulationCore");
		var resource = new ResourceState("preview-resource", ResourceType.VIRELIUM, "spawner", new Vector2(-1000, -1000), 2500);
		((MatchState)simulation.GetState()).AddResource(resource);
		scene.Call("ShowConstructionPreview", "spawn_power_plant");
		var preview = scene.GetChildren().OfType<ConstructionSite>().Single();
		preview.GlobalPosition = resource.CurrentPosition;
		scene.Call("UpdateConstructionPreviewAppearance");
		AssertThat(preview.Modulate).IsEqual(new Color(1f, 0.15f, 0.15f, 0.65f));

		preview.GlobalPosition += new Vector2(300, 0);
		scene.Call("UpdateConstructionPreviewAppearance");
		AssertThat(preview.Modulate).IsEqual(new Color(1f, 1f, 1f, 0.45f));

		preview.GlobalPosition = resource.CurrentPosition;
		resource.Extract(resource.MaxAmount);
		scene.Call("UpdateConstructionPreviewAppearance");
		AssertThat(preview.Modulate).IsEqual(new Color(1f, 1f, 1f, 0.45f));
	}
}
