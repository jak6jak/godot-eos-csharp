#if TOOLS
using Godot;
using System;

[Tool]
public partial class Eosplugin : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		GD.Print("Hello from Eosplugin!");
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}
}
#endif
