using Godot;
using System;

public partial class GameLogic : Node2D
{
	[Export] private Player player;
	[Export] private int score;
	[Export] private Label scoreLabel;

	private CanvasLayer pauseMenu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var cameraNode = player.GetNode("Camera2D");
		pauseMenu = cameraNode.GetNode<CanvasLayer>("PauseMenu");
		scoreLabel = player.Score;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			//PauseScreen
		if (Input.IsActionJustPressed("ui_cancel"))
			if (pauseMenu.IsVisible())
			{
				ContinueGame();
			}
			else
			{
				PauseGame();
			}
		}
	}

	private void PauseGame()
	{
		pauseMenu.Show();
		GetTree().Paused = true;
	}

	public void ContinueGame()
	{
		pauseMenu.Hide();
		GetTree().Paused = false;
	}

	public void AddScore(int scoreToAdd)
	{
		score += scoreToAdd;
		scoreLabel.Text = "Score: " + score;
	}
}
