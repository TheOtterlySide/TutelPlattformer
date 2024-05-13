using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private float Speed;
    private const float JumpVelocity = -400.0f;

    [Export] private bool IsWater;
    [Export] private float WaterSpeed;
    [Export] private float LandSpeed;
    [Export] private float DashCoolDown;
    [Export] private float DashPower;
    [Export] private int OGDoubleJump;

    [Export]
    public PackedScene Bullet { get; set; }
    
    private bool CanDash = true;
    private Timer Timer;
    private int DoubleJump;



    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        Speed = LandSpeed;
        Timer = GetNode<Timer>("DashTimer");
        DoubleJump = OGDoubleJump;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector
        (
            "ui_left",
            "ui_right",
            "ui_up",
            "ui_down"
        );
        
        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            --DoubleJump;
        }
        
        if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && DoubleJump >= 0)
        {
            velocity.Y = JumpVelocity;
            --DoubleJump;
        }

        if (IsOnFloor())
        {
            DoubleJump = OGDoubleJump;
        }

        if (direction != Vector2.Zero)
        {
            velocity.X = direction.X * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        }

        if (Input.IsActionPressed("Fire"))
        {
            Fire();
        }
        
        if (Input.IsActionPressed("Dash") && IsOnFloor())
        {
            if (CanDash)
            {
                velocity.X = PlayerDash(velocity, direction).X;
                CanDash = false;
                Timer.Start();
            }
        }
        
        Velocity = velocity;
        MoveAndSlide();
    }

    public void IsInWater(bool status)
    {
        Speed = status ? WaterSpeed : LandSpeed;
    }

    private Vector2 PlayerDash(Vector2 velocity, Vector2 direction)
    {
        velocity.X = direction.X * Speed * DashPower;
        return velocity;
    }

    private void _on_timer_timeout()
    {
        CanDash = true;
    }

    private void Fire()
    {
        GD.Print("FIRE");
        var instance = Bullet.Instantiate();
        AddChild(instance);
    }
}