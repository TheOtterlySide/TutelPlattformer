using Godot;
using System;

public partial class WaterHitbox : Area2D
{
	[Export] private AnimationPlayer BackgroundTransition;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void EnterChangeBackground(Node2D body)
	{
		if (body.IsInGroup("Player") && body is Player player)
		{
			BackgroundTransition.Play("TransitionToUnderground");
		}
	}

	private void ExitChangeBackground(Node2D body)
	{
		if (body.IsInGroup("Player") && body is Player player)
		{
		}
	}
}
