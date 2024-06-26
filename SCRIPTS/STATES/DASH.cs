using Godot;
using System;

public partial class DASH : State
{
    [Export]
    private Player Player;

    private AnimatedSprite2D PlayerSprite;

    public override void _Ready()
    {
        PlayerSprite = Player.GetNode<AnimatedSprite2D>("PlayerSprite");
    }
	
    public override void Enter() { 
        GD.Print("State hit");	
        PlayerSprite.Play("jump");
    }
    
    public override void Exit() 
    {
            
    }
}