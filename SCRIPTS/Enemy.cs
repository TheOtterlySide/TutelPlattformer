using Godot;
using System;

public partial class Enemy : Area2D
{
	[Export] private int Life;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Enemy");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Destroy()
	{
		QueueFree();
	}

	private void _on_body_entered(Node2D body)
	{
		if (body.IsInGroup("Bullet"))
		{
			LifeHandling();
		}
	}

	private void LifeHandling()
	{
		--Life;
		GD.Print(Life);
		if (Life <= 0)
		{
			QueueFree();
		}
	}
}
