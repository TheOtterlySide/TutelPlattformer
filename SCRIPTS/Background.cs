using Godot;

namespace Tutel.SCRIPTS;

public partial class Background : Node2D
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
	
	private void _on_terrain_detector_background_change(int id)
	{
		switch (id)
		{
			case 0: 
				BackgroundTransition.Play("TransitionToOverworld");
				break;
			case 1:
				BackgroundTransition.Play("TransitionToUnderground");
				break;
		}
	}
}

