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


    [Export] public PackedScene BulletScene { get; set; }

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

    private Tutel.SCRIPTS.StateMachine sfm;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        sfm = new StateMachine();
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
        if (Input.IsActionJustReleased("ui_left") || Input.IsActionJustReleased("ui_right"))
        {
            NewState = State.Idle;
        }
        //Movement
        velocity.X = direction.X * Speed;
        
        
        // if (CanSlide && CurrentState == State.Slide)
        // {
        //     velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        // }

        //Standing Still
        if (velocity.Length() == 0)
        {
            NewState = State.Idle;
        }

        if (velocity.X > 0 && velocity.X < 0)
        {
            NewState = State.Move;
        }
        
        //Wallslide
        if (!IsOnFloor() && IsOnWallOnly())
        {
            WallSlideLogic();
        }

        if (IsOnFloor())
        {
            CheckGround();
        }

        //Not Sliding anymore
        if (IsOnFloor() && !IsOnWallOnly())
        {
            CanSlide = false;
            Gravity = 300;
            WallSlideParticle.Emitting = false;

            if (CurrentState == State.Slide)
            {
                NewState = State.Idle;
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

        // Handle Fall
        if (Input.IsActionJustReleased("ui_accept"))
        {
            NewState = State.Fall;
        }

        //Handle Landing
        if (Input.IsActionJustReleased("ui_accept") && IsOnFloor())
        {
            NewState = State.Idle;
        }

        // JumpJump
        if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && DoubleJump >= 0)
        {
            velocity.Y = JumpVelocity;
            JumpJumpLogic();
        }

        if (IsOnFloor() || IsOnWall())
        {
            ResetDoubleJump();
        }


        if (Input.IsActionPressed("Fire") && CanShoot)
        {
            NewState = State.Shoot;
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

        if (GetGlobalMousePosition().Y > DeathHeight)
        {
            GameOver();
        }

        SetState(velocity);

        Velocity = velocity;
        MoveAndSlide();
    }

    private void SetState(Vector2 velocity)
    {
        Debug.Text = NewState + " " + velocity.X.ToString("0.00");
        switch (NewState)
        {
            case State.Idle:

                SetCurrentState();
                sfm.TransitionTo("IDLE");
                Gun.Play("idle");
                break;

            case State.Move:

                if (CurrentState == State.Idle)
                {
                    SetCurrentState();
                    sfm.TransitionTo("MOVE");
                }

                break;

            case State.Dash:

                if (CurrentState == State.Move || CurrentState == State.Idle)
                {
                    SetCurrentState();
                    sfm.TransitionTo("DASH");
                }

                break;

            case State.Jump:

                SetCurrentState();
                sfm.TransitionTo("JUMP");

                break;

            case State.JumpJump:

                if (CurrentState == State.Jump || CurrentState == State.Fall)
                {
                    SetCurrentState();
                    sfm.TransitionTo("JUMPJUMP");
                }

                break;

            case State.Shoot:

                SetCurrentState();
                Gun.Play("shoot");
                sfm.TransitionTo("SHOOT");

                break;

            case State.Fall:

                if (CurrentState == State.Jump || CurrentState == State.JumpJump)
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

    private void CheckGround()
    {
        
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
        Gun.Position = GunPositionOG;
        WallSlideParticle.Position = WallSlideParticlePositionOG;
        BulletDirection = Vector2.Right;
        NewState = State.Move;
    }

    private void LeftMovementLogic()
    {
        PlayerSprite.FlipH = true;
        Gun.FlipH = true;
        StateMovement = true;
        Gun.Position = GunPosition.Position;
        WallSlideParticle.Position = GunPosition.Position;
        BulletDirection = Vector2.Left;
        NewState = State.Move;
    }

    private void DashLogic()
    {
        CanDash = false;
        NewState = State.Dash;
        DashTimer.Start();
    }

    private void JumpJumpLogic()
    {
        --DoubleJump;
        NewState = State.JumpJump;
    }

    private void JumpLogic()
    {
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
        NewState = State.Jump;
    }

    private void WallSlideLogic()
    {
        Gravity = 300;
        CanSlide = true;
        WallSlideParticle.Emitting = true;
        NewState = State.Slide;
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
    }

    private void Fire(double delta, Vector2 direction)
    {
        if (Ammunition <= 0) return;
        
        Gun.Play("shoot");
        var instance = (Bullet)BulletScene.Instantiate();

        instance.AddToGroup("Bullet");
        instance.Rotation = GlobalRotation;
        instance.Position = new Vector2(GlobalPosition.X + 2, GlobalPosition.Y - 2);
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