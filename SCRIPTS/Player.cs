using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private float Speed;
    [Export] private const float JumpVelocity = -200.0f;

    [Export] private bool IsWater;
    
    [Export] private float WaterSpeed;
    [Export] private float LandSpeed;
    [Export] private float DashCoolDown;
    [Export] private float DashPower;
    
    [Export] private int OGDoubleJump;
    [Export] private int Ammunition;
    [Export] private int Life;


    [Export]
    public PackedScene BulletScene { get; set; }
    
    private bool CanDash = true;
    private bool CanShoot = true;
    private bool CanSlide;
    
    private Timer DashTimer;
    private Timer ShootTimer;
    
    private Sprite2D PlayerSprite;
    private Sprite2D Gun;
    
    private int DoubleJump;

    private Area2D GunPosition;
    private Vector2 GunPositionOG;

    private GpuParticles2D WallSlideParticle;



    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        Speed = LandSpeed;
        BulletScene = (PackedScene)GD.Load("res://OBJECTS/Bullet.tscn");
        Gun = GetNode<Sprite2D>("Watergun");
        GunPositionOG = Gun.Position;
        GunPosition = GetNode<Area2D>("GunPosition");
        DashTimer = GetNode<Timer>("DashTimer");
        ShootTimer = GetNode<Timer>("ShootTimer");
        PlayerSprite = GetNode<Sprite2D>("Sprite2D");
        WallSlideParticle = GetNode<GpuParticles2D>("GPUParticles2D");
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

        if (Input.IsActionPressed("ui_left"))
        {
            PlayerSprite.FlipH = true;
            Gun.FlipH = true;
            Gun.Position = GunPosition.Position;
        }
        
        if (Input.IsActionPressed("ui_right"))
        {
            PlayerSprite.FlipH = false;
            Gun.FlipH = false;
            Gun.Position = GunPositionOG;
        }
        
        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity.Y += gravity * (float)delta;
        }

        if (IsOnWall() && !IsOnFloor())
        {
            gravity = 200;
            CanSlide = true;
            WallSlideParticle.Emitting = true;
        }
        else
        {
            CanSlide = false;
            gravity = 300;
            WallSlideParticle.Emitting = false;
        }

        if (IsOnWall() && !IsOnFloor() && Input.IsActionJustPressed("ui_accept"))
        {
            CanSlide = false;
            PlayerSprite.FlipH = true;
            gravity = 300;
            WallSlideParticle.Emitting = false;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            CanSlide = false;
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

        if (direction != Vector2.Zero && CanSlide == false)
        {
            velocity.X = direction.X * Speed;
            SetRotation();
        }
        else
        {
            if (CanSlide == false)
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                SetRotation();
            }
        }

        if (Input.IsActionPressed("Fire"))
        {
            if (CanShoot)
            {
                Fire(delta, direction);
            }
        }
        
        if (Input.IsActionPressed("Dash") && IsOnFloor())
        {
            if (CanDash)
            {
                velocity.X = PlayerDash(velocity, direction).X;
                CanDash = false;
                DashTimer.Start();
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

    private void Fire(double delta, Vector2 direction)
    {
        if (Ammunition <= 0) return;
        
        var instance = (Bullet)BulletScene.Instantiate();
        
        instance.AddToGroup("Bullet");
        instance.Rotation = GlobalRotation;
        instance.Position = new Vector2(GlobalPosition.X + 2, GlobalPosition.Y);
        instance.LinearVelocity = instance.Transform.X * Speed;
        
        --Ammunition;
        CanShoot = false;
        ShootTimer.Start();
        
        GetTree().Root.AddChild(instance);
    }

    private void _on_shoot_timer_timeout()
    {
        CanShoot = true;
    }

    private void SetRotation()
    {
        PlayerSprite.Rotation = GlobalRotation;
    }
}