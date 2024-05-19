using Godot;
using System;

public partial class Goal : Area2D
{
	private void _on_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			var t = ResourceLoader.Load<PackedScene>("res://LEVEL/Main Menu.tscn");
			GetTree().ChangeSceneToPacked(t);
		}
	}
}
