using Godot;

namespace Tutel.SCRIPTS;

public partial class Collectable : Node2D
{
	[Export] private GameLogic gameLogic;
	[Export] private int Points { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameLogic = (GameLogic)this.GetParent().GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_area_2d_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player") && body is Player player)
		{
			gameLogic.AddScore(Points);
			QueueFree();
		}
	}
}