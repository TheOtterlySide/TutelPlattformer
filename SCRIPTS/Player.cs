using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    enum State
    {
        Idle,
        Walk,
        Jump,
        Shoot
    };

    private State CurrentState;
    
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
    [Export] private float DeathHeight;


    [Export]
    public PackedScene BulletScene { get; set; }
    
    private bool CanDash = true;
    private bool CanShoot = true;
    private bool CanSlide;
    
    private Timer DashTimer;
    private Timer ShootTimer;
    
    private AnimatedSprite2D PlayerSprite;
    private AnimatedSprite2D Gun;
    
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

    private Vector2 BulletDirection = Vector2.Right;


    private bool StateMovement;
    private Vector2 StartPos;


    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        Speed = LandSpeed;
        BulletScene = (PackedScene)GD.Load("res://OBJECTS/Bullet.tscn");
        Gun = GetNode<AnimatedSprite2D>("Watergun");
        GunPositionOG = Gun.Position;
        GunPosition = GetNode<Area2D>("GunPosition");
        DashTimer = GetNode<Timer>("DashTimer");
        ShootTimer = GetNode<Timer>("ShootTimer");
        PlayerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
        WallSlideParticle = GetNode<GpuParticles2D>("GPUParticles2D");
        WallSlideParticlePositionOG = WallSlideParticle.Position;
        DoubleJump = OGDoubleJump;
        AmmoOG = Ammunition;

        CurrentState = State.Idle;

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
            StateMovement = true;
            Gun.Position = GunPosition.Position;
            WallSlideParticle.Position = GunPosition.Position;
            BulletDirection = Vector2.Left;
        }
        
        if (Input.IsActionPressed("ui_right"))
        {
            PlayerSprite.FlipH = false;
            Gun.FlipH = false;
            StateMovement = true;
            Gun.Position = GunPositionOG;
            WallSlideParticle.Position = WallSlideParticlePositionOG;
            BulletDirection = Vector2.Right;
        }

        if (velocity == Vector2.Zero)
        {
            StateMovement = false;
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
            PlayerSprite.Play("jump");
        }
        
        if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && DoubleJump >= 0)
        {
            velocity.Y = JumpVelocity;
            --DoubleJump;
            PlayerSprite.Play("jump");
        }

        if (IsOnFloor() || IsOnWall())
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

        if (velocity.Length() > 0)
        {
            Gun.Play("move");

            if (PlayerSprite.Animation != "jump")
            {
                PlayerSprite.Play("move");
            }
        }
        
        if (velocity.Length() <= 0)
        {
            if (!Gun.IsPlaying() || Gun.Animation == "move")
            {
                Gun.Play("idle");
            }
            if (!PlayerSprite.IsPlaying() || PlayerSprite.Animation == "move")
            {
                PlayerSprite.Play("idle");
            }
        }
        
        if (Input.IsActionPressed("Fire") && CanShoot)
        {
            Gun.Play("shoot");
            Fire(delta, direction);
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

        if (GetGlobalMousePosition().Y > DeathHeight)
        {
            GameOver();
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
        Gun.Play("shoot");
        var instance = (Bullet)BulletScene.Instantiate();
        
        instance.AddToGroup("Bullet");
        instance.Rotation = GlobalRotation;
        instance.Position = new Vector2( GlobalPosition.X + 2, GlobalPosition.Y - 2);
        instance.LinearVelocity = instance.Transform.X * BulletDirection * Speed;
        --Ammunition;
        DeactivateHUDAmmo(Ammunition);
        CanShoot = false;
        ShootTimer.Start();
        
        GetTree().Root.AddChild(instance);
    }

    private void _on_shoot_timer_timeout()
    {
        CanShoot = true;
    }

    public void Reload()
    {
        Ammunition = AmmoOG;
        ActivateHUDAmmo(Ammunition);
    }
    private void DeactivateHUDAmmo(int countActive)
    {
        for (int i = countActive; i < AmmoList.Count; i++)
        {
            AmmoList[i].Visible = false;
        }
    }

    private void ActivateHUDAmmo(int countActive)
    {
        foreach (var ammoSprite in AmmoList)
        {
            ammoSprite.Visible = true;
        }
    }

    public void GameOver()
    {
        GlobalPosition = StartPos;
        GetTree().CallDeferred("reload_current_scene");
    }

    private void GameEnd()
    {
        var t = ResourceLoader.Load<PackedScene>("res://LEVEL/Main Menu.tscn");
        GetTree().ChangeSceneToPacked(t);
    }
}