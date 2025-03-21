using Godot;
using System;

public partial class Enemy : Area2D
{
	[Export] private int Life;
	[Export] private Vector2 Direction;
	[Export] private float Speed;

	private Vector2 StartPos;

	private Vector2 DestinationPos;
	private AnimatedSprite2D AnimatedSprite;

	private bool IsMoving;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Enemy");
		StartPos = GlobalPosition;
		DestinationPos = StartPos + Direction;
		AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		IsMoving = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (IsMoving)
		{
			GlobalPosition = GlobalPosition.MoveToward(DestinationPos, Speed * (float)delta);
			AnimatedSprite.Play("moving");
			if (GlobalPosition == DestinationPos)
			{
				if (GlobalPosition == StartPos)
				{
					DestinationPos = StartPos + Direction;
				}
				else
				{
					DestinationPos = StartPos;
				}
			}
		}
	}
	
	private void _on_body_entered(Node2D body)
	{
		if (body.IsInGroup("Bullet"))
		{
			IsMoving = false;
			LifeHandling();
		}
		if (body.IsInGroup("Player") && body is Player player)
		{
			player.GameOver();
		}
	}

	private void LifeHandling()
	{
		--Life;
		AnimatedSprite.Stop();
		AnimatedSprite.Play("hurt");
		if (Life <= 0)
		{
			QueueFree();
		}
	}

	private void _on_hurt_finished()
	{
		IsMoving = true;
	}
}
