using Godot;
using System;

public partial class GameLogic : Node2D
{
	[Export] private Player player;
	[Export] private Node2D pauseMenu;
	[Export] private int score;
	[Export] private Label scoreLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
		GD.Print("AddScore: " + score);
		score += scoreToAdd;
		scoreLabel.Text = "Score: " + score;
	}
}
