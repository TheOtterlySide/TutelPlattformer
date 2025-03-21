using System;
using Godot;
using System.Collections.Generic;
using Tutel.SCRIPTS;

public partial class Player : CharacterBody2D
{
    enum State
    {
        Idle,
        Move,
        Jump,
        Shoot,
        JumpJump,
        Slide,
        Dash,
        Fall
    };

    private State NewState;
    private State CurrentState;


    private float Speed;
    [Export] private float BulletSpeed;
    [Export] private float JumpVelocity = -300.0f;

    [Export] private bool IsWater;

    [Export] private float WaterSpeed;
    [Export] private float LandSpeed;
    [Export] private float DashCoolDown;
    [Export] private float DashPower;

    [Export] private int OGDoubleJump;
    [Export] private int Ammunition;
    [Export] private int Life;
    [Export] private float DeathHeight;

    [Export] private Label Debug;
    [Export] public Label Score;


    [Export] public PackedScene BulletScene { get; set; }

    private bool CanDash = true;
    private bool CanShoot = true;
    private bool CanSlide;

    private Timer DashTimer;
    private Timer ShootTimer;
    private Timer JumpTimer;

    private AnimatedSprite2D PlayerSprite;
    private AnimatedSprite2D Gun;

    private int DoubleJump;

    #region Ammo Related

    private Sprite2D HUDAmmo1;
    private Sprite2D HUDAmmo2;
    private Sprite2D HUDAmmo3;
    private Sprite2D HUDAmmo4;
    private Sprite2D HUDAmmo5;
    private List<Sprite2D> AmmoList = new List<Sprite2D>();
    private int AmmoOG;

    #endregion

    private Area2D GunPositionLeft;
    private Area2D GunPositionBottom;
    private Area2D GunPositionBottomLeft;
    private Vector2 GunPositionOG;

    private GpuParticles2D WallSlideParticle;
    private Vector2 WallSlideParticlePositionOG;

    private Vector2 BulletDirection = Vector2.Right;
    private Bullet instance;


    private bool StateMovement;
    private Vector2 StartPos;

    private StateMachine sfm;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        sfm = new StateMachine();
        Speed = LandSpeed;
        BulletScene = (PackedScene)GD.Load("res://OBJECTS/Bullet.tscn");
        Gun = GetNode<AnimatedSprite2D>("Watergun");
        GunPositionOG = Gun.Position;
        GunPositionLeft = GetNode<Area2D>("GunPositionLeft");
        GunPositionBottom = GetNode<Area2D>("GunPositionBottom");
        GunPositionBottomLeft = GetNode<Area2D>("GunPositionBottomLeft");
        DashTimer = GetNode<Timer>("Timer/DashTimer");
        ShootTimer = GetNode<Timer>("Timer/ShootTimer");
        JumpTimer = GetNode<Timer>("Timer/JumpTimer");
        PlayerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
        WallSlideParticle = GetNode<GpuParticles2D>("GPUParticles2D");
        WallSlideParticlePositionOG = WallSlideParticle.Position;
        DoubleJump = OGDoubleJump;
        AmmoOG = Ammunition;

        NewState = State.Idle;
        CurrentState = State.Idle;

        SetupHUD();
    }

    private void SetupHUD()
    {
        HUDAmmo1 = GetNode<Sprite2D>("Camera2D/HUD/Control/1");
        HUDAmmo2 = GetNode<Sprite2D>("Camera2D/HUD/Control/2");
        HUDAmmo3 = GetNode<Sprite2D>("Camera2D/HUD/Control/3");
        HUDAmmo4 = GetNode<Sprite2D>("Camera2D/HUD/Control/4");
        HUDAmmo5 = GetNode<Sprite2D>("Camera2D/HUD/Control/5");

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
        Vector2 direction = Input.GetVector
        (
            "ui_left",
            "ui_right",
            "ui_up",
            "ui_down"
        );

        // Add the gravity.
        velocity.Y += Gravity * (float)delta;

        if (Input.IsActionPressed("ui_left"))
        {
            LeftMovementLogic();
        }

        if (Input.IsActionPressed("ui_right"))
        {
            RightMovementLogic();
        }

        //Standing still
        if ((Input.IsActionJustReleased("ui_left") && IsOnFloor()) || (Input.IsActionJustReleased("ui_right") && IsOnFloor()))
        {
            NewState = State.Idle;
        }

        //Movement
        velocity.X = direction.X * Speed;

        //Standing Still
        if (velocity.Length() == 0)
        {
            NewState = State.Idle;
        }

        if (velocity.X > 0 || velocity.X < 0)
        {
            if (IsOnFloor())
            {
                NewState = State.Move;
            }
        }

        //Wallslide
        if (!IsOnFloor() && IsOnWallOnly())
        {
            WallSlideLogic();
        }

        //Not Sliding anymore
        if (IsOnFloor() && !IsOnWall() || !IsOnFloor() && !IsOnWall())
        {
            CanSlide = false;
            Gravity = 300;
            WallSlideParticle.Emitting = false;
            Gun.Rotation = 0;

            if (CurrentState == State.Slide && IsOnFloor())
            {
                NewState = State.Idle;
            }

            if (CurrentState == State.Slide && !IsOnFloor())
            {
                NewState = State.Fall;
            }
        }

        //Wallslide Jump
        if (CurrentState == State.Slide && Input.IsActionJustPressed("ui_accept"))
        {
            WallSlideJumpLogic();
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            JumpLogic();
        }

        //Handle Landing
        if (CurrentState == State.Fall && IsOnFloor())
        {
            NewState = State.Idle;
        }

        // JumpJump
        if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && !IsOnWall() && DoubleJump >= 0)
        {
            velocity.Y = JumpVelocity;
            JumpJumpLogic();
        }

        if (IsOnFloor() || IsOnWall())
        {
            ResetDoubleJump();
        }

        if (!IsOnFloor() && !IsOnWall())
        {
            if (CurrentState == State.Move)
            {
                NewState = State.Fall;
            }
        }

        if (Input.IsActionJustPressed("Fire") && CanShoot)
        {
            Fire(delta, direction);
        }

        if (Input.IsActionPressed("Dash"))
        {
            if (CanDash)
            {
                velocity.X = PlayerDash(velocity, direction).X;
                DashLogic();
            }
        }

        if (GetGlobalPosition().Y > DeathHeight)
        {
            GameOver();
        }

        SetState();
        Debug.Text = CurrentState.ToString();
        Velocity = velocity;
        MoveAndSlide();
    }

    private void SetState()
    {
        switch (NewState)
        {
            case State.Idle:

                SetCurrentState();
                sfm.TransitionTo("IDLE");
                break;

            case State.Move:

                if (CurrentState == State.Idle)
                {
                    SetCurrentState();
                    sfm.TransitionTo("MOVE");
                }

                break;

            case State.Dash:

                SetCurrentState();
                sfm.TransitionTo("DASH");

                break;

            case State.Jump:

                SetCurrentState();
                sfm.TransitionTo("JUMP");

                break;

            case State.JumpJump:

                if (CurrentState == State.Jump || CurrentState == State.Fall || CurrentState == State.Slide)
                {
                    SetCurrentState();
                    sfm.TransitionTo("JUMPJUMP");
                }

                break;

            case State.Shoot:

                // SetCurrentState();
                // sfm.TransitionTo("SHOOT");
                break;

            case State.Fall:

                if (CurrentState == State.Jump || CurrentState == State.JumpJump || CurrentState == State.Dash || CurrentState == State.Move || CurrentState == State.Slide)
                {
                    SetCurrentState();
                    sfm.TransitionTo("FALL");
                }

                break;

            case State.Slide:

                if (CurrentState == State.Jump || CurrentState == State.JumpJump || CurrentState == State.Fall)
                {
                    if (IsOnWall())
                    {
                        SetCurrentState();
                        sfm.TransitionTo("SLIDE");
                    }
                }

                break;

            default:
                sfm.TransitionTo("IDLE");
                break;
        }
    }

    private void ResetDoubleJump()
    {
        DoubleJump = OGDoubleJump;
    }

    #region Logic

    private void RightMovementLogic()
    {
        PlayerSprite.FlipH = false;
        Gun.FlipH = false;
        StateMovement = true;
        if (CurrentState == State.Slide)
        {
            Gun.Position = GunPositionOG;
        }

        WallSlideParticle.Position = WallSlideParticlePositionOG;
        BulletDirection = Vector2.Right;
    }

    private void LeftMovementLogic()
    {
        PlayerSprite.FlipH = true;
        Gun.FlipH = true;
        StateMovement = true;
        if (CurrentState != State.Slide)
        {
            Gun.Position = GunPositionLeft.Position;
        }

        WallSlideParticle.Position = GunPositionLeft.Position;
        BulletDirection = Vector2.Left;
    }

    private void DashLogic()
    {
        CanDash = false;
        NewState = State.Dash;
        DashTimer.Start();
    }

    private void JumpJumpLogic()
    {
        JumpTimer.Start();
        --DoubleJump;
        NewState = State.JumpJump;
    }

    private void JumpLogic()
    {
        JumpTimer.Start();
        CanSlide = false;
        --DoubleJump;
        NewState = State.Jump;
    }

    private void WallSlideJumpLogic()
    {
        CanSlide = false;
        PlayerSprite.FlipH = true;
        Gravity = 300;
        WallSlideParticle.Emitting = false;

        JumpTimer.Start();
        NewState = State.Jump;
    }

    private void WallSlideLogic()
    {
        Gravity = 300;
        CanSlide = true;
        WallSlideParticle.Emitting = true;
        NewState = State.Slide;
        
        if (Input.IsActionPressed("ui_left"))
        {
            Gun.RotationDegrees = -90;
            Gun.Position = GunPositionBottomLeft.Position;
        }

        if (Input.IsActionPressed("ui_right"))
        {
            Gun.RotationDegrees = 90;
            Gun.Position = GunPositionBottom.Position;
        }
    }

    #endregion


    private void SetCurrentState()
    {
        CurrentState = NewState;
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
        DashTimer.Stop();
    }

    private void Fire(double delta, Vector2 direction)
    {
        if (Ammunition <= 0) return;

        instance = (Bullet)BulletScene.Instantiate();
        Gun.Play("shoot");
        instance.AddToGroup("Bullet");
        instance.Rotation = GlobalRotation;
        instance.Position = new Vector2(GlobalPosition.X + 2, GlobalPosition.Y - 2);
        instance.LinearVelocity = instance.Transform.X * BulletDirection * BulletSpeed;
        --Ammunition;
        DeactivateHUDAmmo(Ammunition);
        CanShoot = false;
        ShootTimer.Start();

        GetTree().Root.AddChild(instance);
    }

    private void _on_shoot_timer_timeout()
    {
        CanShoot = true;
        ShootTimer.Stop();
    }

    private void _on_shoot_animation_finished()
    {
        Gun.Play("idle");
    }

    private void _on_jump_timer_timeout()
    {
        NewState = State.Fall;
        JumpTimer.Stop();
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