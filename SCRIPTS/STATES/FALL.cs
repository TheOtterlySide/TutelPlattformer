using Godot;
using System;

public partial class FALL : State
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
		PlayerSprite.Play("fall");
	}
    
	public override void Exit() 
	{
            
	}
}
