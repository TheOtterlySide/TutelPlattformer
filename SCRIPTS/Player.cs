using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

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

    private Sprite2D HUDAmmo1;
    private Sprite2D HUDAmmo2;
    private Sprite2D HUDAmmo3;
    private Sprite2D HUDAmmo4;
    private Sprite2D HUDAmmo5;
    private List<Sprite2D> AmmoList = new List<Sprite2D>();
    private int AmmoOG;

    private Area2D GunPosition;
    private Vector2 GunPositionOG;

    private GpuParticles2D WallSlideParticle;
    private Vector2 WallSlideParticlePositionOG;



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
        WallSlideParticlePositionOG = WallSlideParticle.Position;
        DoubleJump = OGDoubleJump;
        AmmoOG = Ammunition;

        HUDAmmo1 = GetNode<Sprite2D>("HUD/1");
        HUDAmmo2 = GetNode<Sprite2D>("HUD/2");
        HUDAmmo3 = GetNode<Sprite2D>("HUD/3");
        HUDAmmo4 = GetNode<Sprite2D>("HUD/4");
        HUDAmmo5 = GetNode<Sprite2D>("HUD/5");
        SetupHUD();
    }

    private void SetupHUD()
    {
        AmmoList.Add(HUDAmmo1);
        AmmoList.Add(HUDAmmo2);
        AmmoList.Add(HUDAmmo3);
        AmmoList.Add(HUDAmmo4);
        AmmoList.Add(HUDAmmo5);
        
        foreach (var node in AmmoList)
        {
            node.Visible = true;
        }
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
            WallSlideParticle.Position = GunPosition.Position;
        }
        
        if (Input.IsActionPressed("ui_right"))
        {
            PlayerSprite.FlipH = false;
            Gun.FlipH = false;
            Gun.Position = GunPositionOG;
            WallSlideParticle.Position = WallSlideParticlePositionOG;
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
        }
        else
        {
            if (CanSlide == false)
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
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
        HandleAmmo();
        
        GetTree().Root.AddChild(instance);
    }

    private void _on_shoot_timer_timeout()
    {
        CanShoot = true;
    }

    private void HandleAmmo()
    {
        switch (Ammunition)
        {
            case 5:
                HUDAmmo1.Visible = false;
                break;
            case 4:
                ChangeHUDAmmo(4);
                break;
            case 3:                
                ChangeHUDAmmo(3);
                break;
            case 2:
                ChangeHUDAmmo(2);
                break;
            case 1:
                ChangeHUDAmmo(1);
                break;
            case 0:
                ChangeHUDAmmo(0);
                break;
            default:
                break;
        }
    }

    private void ChangeHUDAmmo(int countActive)
    {
        GD.Print("Handling Ammo " + countActive);

        for (int i = 0; i < countActive; i++)
        {
            GD.Print("For" + countActive);
            AmmoList[i].Visible = false;
        }

        if (countActive != AmmoOG)
        {
            var disc = AmmoOG - countActive;
            GD.Print("disc" + countActive);

            for (int i = disc; i < AmmoOG; i++)
            {
                AmmoList[i].Visible = false;
            }
        }
    }
}