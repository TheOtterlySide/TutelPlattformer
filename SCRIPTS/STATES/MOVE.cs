using Godot;
using System;

public partial class MOVE : State
{
	[Export]
	private Player Player;

	private AnimatedSprite2D PlayerSprite;

	public override void _Ready()
	{
		PlayerSprite = Player.GetNode<AnimatedSprite2D>("PlayerSprite");
	}
	
	public override void Enter() 
	{ 
		PlayerSprite.Play("move");
	}
    
	public override void Exit() 
	{
            
	}
	
}
