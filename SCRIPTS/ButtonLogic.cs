using Godot;
using System;

public partial class ButtonLogic : Button
{
	private void _on_pressed()
	{
		var t = ResourceLoader.Load<PackedScene>("res://LEVEL/level_1.tscn");
		GetTree().ChangeSceneToPacked(t);
	}

	private void _on_exit_pressed()
	{
		GetTree().Quit();
	}
}
