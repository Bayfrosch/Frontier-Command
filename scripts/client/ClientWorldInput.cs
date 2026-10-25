using Godot;

public static class ClientWorldInput
{
	public static void IgnoreGuiMouse(Node node)
	{
		if (node is Control control)
			control.MouseFilter = Control.MouseFilterEnum.Ignore;

		foreach (var child in node.GetChildren())
			IgnoreGuiMouse(child);
	}
}
