using Godot;
using System;

public partial class ButtonLogic : Button
{
	[Export] private GameLogic pauseMenu;

	private void _on_pressed()
	{
		var t = ResourceLoader.Load<PackedScene>("res://LEVEL/level_1.tscn");
		GetTree().ChangeSceneToPacked(t);
	}

	private void _on_exit_pressed()
	{
		GetTree().Quit();
	}

	private void _on_MainMenu_pressed()
	{
		var t = ResourceLoader.Load<PackedScene>("res://LEVEL/Main Menu.tscn");
		GetTree().ChangeSceneToPacked(t);
	}
	private void _on_resume_pressed()
	{
		pauseMenu.ContinueGame();
	}
}
