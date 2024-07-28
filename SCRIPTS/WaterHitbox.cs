using Godot;

namespace Tutel.SCRIPTS;

public partial class WaterHitbox : Area2D
{
	[Export] private AnimationPlayer BackgroundTransition;

	[Export] private bool UndergroundStatus;
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
		if (body.IsInGroup("Player") && !UndergroundStatus)
		{
			UndergroundStatus = true;
			BackgroundTransition.Play("TransitionToUnderground");
			return;
		}

		if (body.IsInGroup("Player") && UndergroundStatus)
		{			
			UndergroundStatus = false;
			BackgroundTransition.Play("TransitionToOverworld");
		}
		
	}
	
}