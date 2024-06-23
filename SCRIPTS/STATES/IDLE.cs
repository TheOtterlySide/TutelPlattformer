using Godot;
using System;

public partial class IDLE : State
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
        PlayerSprite.Play("idle");
    }
    
    public override void Exit() 
        {
            
        }

}