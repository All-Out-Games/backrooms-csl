using AO;

public partial class MyPlayer : Player
{
    public static Texture StamBarBack = Assets.KeepLoaded<Texture>("UI/Health_Bar/health_bar_yellow/healthbar_back.png");
    public static Texture StamBarFill = Assets.KeepLoaded<Texture>("UI/Health_Bar/health_bar_yellow/healthbar_fill.png");

    public bool IsDead => HasEffect<SpectatorEffect>();
    
    public SyncVar<bool> Mobile = new();

    private string _currentItemID = new("");
    public string CurrentItemInHand
    {
        get => _currentItemID;
        set
        {
            if (Network.IsClient)
            {
                _currentItemID = value;
            }
        }
    }
    string oldItemInHand = "";

    public static readonly int DefaultMaxHealth = 100;
    private SyncVar<int> _currentHealth = new(DefaultMaxHealth);
    public int CurrentHealth
    {
        get => _currentHealth.Value;
        set
        {
            if (Network.IsServer)
            {
                _currentHealth.Set(value);
            }
        }
    }

    public float CurrentStamina = 100;
    public float MaxStamina = 100;

    public SyncVar<bool> Sprinting = new(false);
    public SyncVar<int> PickedUpAbility = new(0);

    public SpineInstance ReadyGo;

    public int Wins
    {
        get
        {
            if (Network.IsClient) return 0;
            return Save.GetInt(this, "wins", 0);
        }

        set
        {
            if (Network.IsServer)
            {
                Save.SetInt(this, "wins", value);
                Save.OrderedSet("wins", $"{this.UserId}", value);
            }
        }
    }
  

    private SyncVar<int> _playerRole = new((int)PlayerRole.Spectator);
    public PlayerRole PlayerRole
    {
        get => (PlayerRole)_playerRole.Value;
        set => _playerRole.Set((int)value);
    }

    private SyncVar<int> _playerRoleAtEnd = new((int)PlayerRole.Spectator);
    public PlayerRole PlayerRoleAtEndOfRound
    {
        get => (PlayerRole)_playerRoleAtEnd.Value;
        set => _playerRoleAtEnd.Set((int)value);
    }


    public SyncVar<float> CurrentZoomLevel = new(1.5f);

    public SyncVar<bool> ShadowsEnabled = new(true);

    public CameraControl CameraControl;

    public bool ActionCam = false;

    public bool FocusingUI = false;
    public bool FocusingUIVignette = false;
    public float CurrentFocusUI = 1f;
    public float CurrentFocusUIVelocity = 0f;
    public float CurrentFocusUIVignette = 0f;
    public float CurrentFocusUIVignetteVelocity = 0f;
    public float FlashAlpha = 0f;
    public float FlashAlphavel = 0f;
    public Vector4 FlashColor = new Vector4(1f, 0f, 0f, 1f);

    public float CurrentCamSlider;
    public float CurrentCamVelocity = 0f;

    public SyncVar<float> CheatSpeedMultiplier = new(1);

    public Entity SecondaryCamTarget;
    
    // Camera zoom tuning for tracking the Ball
    public float CameraZoomMin = 1.4f;
    public float CameraZoomMax = 5f;
    public float BallZoomDistanceScale = 0.1f;

    // Group camera helpers
    public bool TryGetTeamAndBallBounds(out Vector2 center, out float radius)
    {
        center = Vector2.Zero;
        radius = 0f;
        int count = 0;

        foreach (var p in Scene.Components<MyPlayer>())
        {
            if (!p.Alive()) continue;
            if (p.PlayerRole == PlayerRole.RedTeam || p.PlayerRole == PlayerRole.BlueTeam)
            {
                center += p.Entity.Position + new Vector2(0, 0.5f);
                count++;
            }
        }

        foreach (var b in Scene.Components<Ball>())
        {
            if (!b.Alive()) continue;
            center += b.Entity.Position;
            count++;
        }

        if (count == 0) return false;

        center /= count;

        float maxDist = 0f;
        foreach (var p in Scene.Components<MyPlayer>())
        {
            if (!p.Alive()) continue;
            if (p.PlayerRole == PlayerRole.RedTeam || p.PlayerRole == PlayerRole.BlueTeam)
            {
                var pos = p.Entity.Position + new Vector2(0, 0.5f);
                float d = (pos - center).Length;
                if (d > maxDist) maxDist = d;
            }
        }
        foreach (var b in Scene.Components<Ball>())
        {
            if (!b.Alive()) continue;
            float d = (b.Entity.Position - center).Length;
            if (d > maxDist) maxDist = d;
        }

        radius = maxDist;
        return true;
    }

    public bool IsInMyGoalBounds()
    {
        var box = PlayerRole == PlayerRole.RedTeam ? GameManager.Instance.RedGoalBounds : GameManager.Instance.BlueGoalBounds;
        if (Position.X > box.Position.X - (box.Size.X / 2) &&
            Position.X < box.Position.X + (box.Size.X / 2) &&
            Position.Y > box.Position.Y - (box.Size.Y / 2) &&
            Position.Y < box.Position.Y + (box.Size.Y / 2))
            return true;

        return false;
    }

    public List<string> HideHudReasons = new List<string>();
    public SyncVar<Entity> PlayerCorpse = new();

    public Vector2 Dash = Vector2.Zero;
    protected float DashRemainingDuration;
    protected const float DashDecayThreshold = 0.1f;

    public float TimeIdleInSeconds = 0;

    public BillboardSign NearSign;

    public Spine_Animator WorldOverrideSkeleton;
    protected Vector2 SkeletonScaleOriginal;

    private SyncVar<float> _scaleMod = new(1f);
    public float ScaleMod
    {
        get => _scaleMod.Value;
        set
        {
            if (Network.IsServer)
            {
                _scaleMod.Set(value);
            }
        }
    }
    public float CScaleMod = 1f;
    public Entity StoreEntity;

    public override void Awake()
    {
        if (IsLocal)
        {
            CameraControl = CameraControl.Create(1);
            CameraControl.Zoom = CurrentZoomLevel.Value;

            ReadyGo = SpineInstance.Make();
            ReadyGo.SetSkeleton(References.Instance.ReadyGoAsset);
            ReadyGo.SetAnimation("animation", false);
        }

        if (Network.IsClient)
        {
            var overrideSkeletonEntity = Entity.Create();
            overrideSkeletonEntity.SetParent(Entity, false);
            SkeletonScaleOriginal = new Vector2(0.1f, 0.1f);
            WorldOverrideSkeleton = overrideSkeletonEntity.AddComponent<Spine_Animator>();
            WorldOverrideSkeleton.Entity.LocalScale = SkeletonScaleOriginal;
            WorldOverrideSkeleton.Entity.LocalEnabled = false;
        }

        // movement agent:
        Agent.CustomVelocityCallback = (agent, currentVelocity, input, dt) =>
        {
            float speed = 250 * CheatSpeedMultiplier;

            //if (HasEffect<EffectSlowed>()) speed *= 0.5f;
            //if (HasEffect<EffectBattleArcher>()) speed *= 1.5f;

            if (GameManager.Instance.State == GameState.SetUp)
            {
                speed = 0;
            }

            if (isSprinting == true)
            {
                speed *= 1.25f;
            }

            MyPlayer player = agent.Entity.GetComponent<MyPlayer>();

            currentVelocity += Dash * dt;

            if (Network.IsServer)
            {
                if (IsDead == false && currentVelocity == Vector2.Zero)
                {
                    var timeBefore = TimeIdleInSeconds;
                    TimeIdleInSeconds += Time.DeltaTime;
                    if ((int)timeBefore == 239 && (int)TimeIdleInSeconds == 240)
                    {
                        CallClient_ShowNotificationLocal("You will be kicked due to inactivity in 1 minute unless you move.");
                    }
                }
                else
                {
                    TimeIdleInSeconds = 0;
                }
            }

            currentVelocity += input * dt * speed;
            return currentVelocity;
        };

        CurrentZoomLevel.OnSync += (old, value) =>
        {
        };

        _playerRole.OnSync += (old, value) =>
        {
            RemoveEffect<SpectatorEffect>(false);
            if (PlayerRole == PlayerRole.Spectator)
            {
                AddEffect<SpectatorEffect>();
            }
        };


        // Base player spine
        {
            StateMachineVariable mIKBool = null;
            StateMachineVariable movingBool = null;
            StateMachineVariable sprintingBool = null;

            var gameLayer = SpineAnimator.SpineInstance.StateMachine.CreateLayer("game_layer", 10);
            var empty = gameLayer.CreateState("__CLEAR_TRACK__", 0, true);

            var aoLayer = SpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("main");
            var aoIdleState = aoLayer.TryGetStateByName("Idle");
            var aoRunState = aoLayer.TryGetStateByName("Run_Fast");
            var aoSprintState = aoLayer.CreateState("SL/Sprint", 0, true);

            mIKBool = SpineAnimator.SpineInstance.StateMachine.CreateVariable("mIK", StateMachineVariableKind.BOOLEAN);
            movingBool = SpineAnimator.SpineInstance.StateMachine.TryGetVariableByName("moving");
            sprintingBool = SpineAnimator.SpineInstance.StateMachine.CreateVariable("sprinting", StateMachineVariableKind.BOOLEAN);

            //Entry
            aoLayer.CreateGlobalTransition(aoIdleState, false).CreateTriggerCondition(SpineAnimator.SpineInstance.StateMachine.CreateVariable("cancel_all", StateMachineVariableKind.TRIGGER));
            aoLayer.CreateTransition(aoRunState, aoSprintState, false).CreateBoolCondition(sprintingBool, true);
            aoLayer.CreateTransition(aoSprintState, aoRunState, false).CreateBoolCondition(sprintingBool, false);

            var teleportLand = gameLayer.CreateState("Teleport_Appear", 0, false);
            var teleportLandTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("teleport_land", StateMachineVariableKind.TRIGGER);
            gameLayer.CreateGlobalTransition(teleportLand).CreateTriggerCondition(teleportLandTrigger);
            gameLayer.CreateTransition(teleportLand, empty, true);
         
            var punchMik = gameLayer.CreateState("CTF/punch_mIK", 0, false);
            var punchTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("punch_mIK", StateMachineVariableKind.TRIGGER);
            gameLayer.CreateGlobalTransition(punchMik).CreateTriggerCondition(punchTrigger);
            gameLayer.CreateTransition(punchMik, empty, true);

            var smallKick = gameLayer.CreateState("SL/Small_Kick", 0, false);
            var smallKickTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("small_kick", StateMachineVariableKind.TRIGGER);
            gameLayer.CreateGlobalTransition(smallKick).CreateTriggerCondition(smallKickTrigger);
            gameLayer.CreateTransition(smallKick, empty, true);

            gameLayer.InitialState = (empty);
            aoLayer.InitialState = aoIdleState;

            //gameLayer.AddSimpleTriggeredState("dodge_roll", "Dodge_Roll", true, false, false);
        }      

        var collisionEntity = Assets.GetAsset<Prefab>("PlayerCollision.prefab").Instantiate();
        collisionEntity.GetComponent<PlayerCollisionChild>().Player = this;
        collisionEntity.LocalScale = new Vector2(1.1f, 1.1f);
        collisionEntity.SetParent(Entity, false);

        if (Network.IsServer)
        {
            LoadPlayerData();
        }

        Sprinting.OnSync += (old, value) =>
        {
            if (Network.IsClient)
            {
                if (SpineAnimator.Alive())
                {
                    if (SpineAnimator.SpineInstance != null && SpineAnimator.SpineInstance.StateMachine != null)
                    {
                        SpineAnimator.SpineInstance.StateMachine.SetBool("sprinting", value);
                    }
                }
            }
        };
    }

    public void LoadPlayerData()
    {
        // Load saved data for players here
    }

    public void MakeReadyGo()
    {
        ReadyGo = SpineInstance.Make();
        ReadyGo.SetSkeleton(References.Instance.ReadyGoAsset);
        ReadyGo.SetAnimation("animation", false);
    }

    public void SetWorldOverrideSkeleton(string id)
    {
        if (Network.IsServer) { return; }

        if (WorldOverrideSkeleton.Alive())
        {
            WorldOverrideSkeleton.Entity.Destroy();
        }
        WorldOverrideSkeleton = null;

        var woStateMachine = StateMachine.Make(); ;
        var woLayer = woStateMachine.CreateLayer("woLayer", 0);

        var woEntity = Entity.Create();
        woEntity.SetParent(Entity, false);
        WorldOverrideSkeleton = woEntity.AddComponent<Spine_Animator>();
        WorldOverrideSkeleton.Awaken();
        StateMachineVariable movingBool = null;

        switch (id)
        {
            case "Chicken":
                WorldOverrideSkeleton.SpineInstance.SetSkeleton(Assets.GetAsset<SpineSkeletonAsset>("animations/chicken/FUSE105_chicken.spine"));
                WorldOverrideSkeleton.MaskInShadow = true;

                movingBool = woStateMachine.CreateVariable("moving", StateMachineVariableKind.BOOLEAN);
                var idleState = woLayer.CreateState("FUSE105/idle", 0, true);
                var runState = woLayer.CreateState("FUSE105/run", 0, true);
                woLayer.CreateTransition(idleState, runState, false).CreateBoolCondition(movingBool, true);
                woLayer.CreateTransition(runState, idleState, false).CreateBoolCondition(movingBool, false);
                woLayer.InitialState = (idleState);

                WorldOverrideSkeleton.SpineInstance.SetStateMachine(woStateMachine, woEntity);
                WorldOverrideSkeleton.SpineInstance.Scale = new Vector2(2, 2);
                WorldOverrideSkeleton.LocalEnabled = true;
                break;

            default:
                Log.Warn("Invalid ID given to Override Skeleton");
                return;

        }
        AddInvisibilityReason("OverrideSkeleton");
    }

    public void RemoveWorldOverrideSkeleton()
    {
        if (Network.IsServer) { return; }

        RemoveInvisibilityReason("OverrideSkeleton");
        WorldOverrideSkeleton.Entity.LocalEnabled = false;
        SpineAnimator.SpineInstance.SetStateMachine(SpineAnimator.SpineInstance.StateMachine, null);
    }


    bool start = false;
    public override void Update()
    {
        if (Util.OneTime(start == false, ref start))
        {
            if (Network.IsServer)
            {
                if (GameManager.Instance.State == GameState.WaitingForPlayers)
                {
                    PlayerRole = PlayerRole.RedTeam;
                }

                if (GameManager.Instance.State != GameState.WaitingForPlayers)
                {
                    GameManager.Instance.AssignPlayerARole(this);
                }
            }

            if (Network.IsClient && IsLocal)
            {
                CallServer_SetIsMobile(Game.IsMobile);
            }

        }

        DrawAbilities();
        if (Network.IsClient && IsLocal)
        {
            DrawSpecialButtons();
        }

        // Player light
        if (IsLocal)
        {
            var currentCamera = CameraControl.GetCurrent();
            if (currentCamera != CameraControl)
            {
                CameraControl.Zoom = AOMath.Lerp(CameraControl.Zoom, currentCamera.Zoom, 10 * Time.DeltaTime);
            }
            else
            {
                if (ActionCam)
                {
                    // Dynamic zoom to include all Red/Blue players and Balls if present
                    if (TryGetTeamAndBallBounds(out var groupCenter, out var groupRadius))
                    {
                        SecondaryCamTarget = null;
                        float desiredZoom = CurrentZoomLevel + (groupRadius * BallZoomDistanceScale);
                        desiredZoom = (float)Math.Clamp(desiredZoom, CameraZoomMin, CameraZoomMax);
                        if (Mobile.Value == true) desiredZoom *= 0.7f;
                        CameraControl.Zoom = AOMath.Lerp(CameraControl.Zoom, desiredZoom, 5 * Time.DeltaTime);
                    }
                    else
                    {
                        // Fallback to player-only zoom
                        CameraControl.Zoom = AOMath.Lerp(CameraControl.Zoom, CurrentZoomLevel, 10 * Time.DeltaTime);
                    }
                }
                else
                {
                    CameraControl.Zoom = AOMath.Lerp(CameraControl.Zoom, CurrentZoomLevel, 10 * Time.DeltaTime);
                }
            }

            DrawCamButton();

            if (HideHudReasons.Count == 0 && GameManager.Instance.State == GameState.Round)
            {

            }
        }

        bool moving = GetComponent<Movement_Agent>().Velocity.Length > 0.03f;

        if (WorldOverrideSkeleton.Alive() && WorldOverrideSkeleton.SpineInstance != null && WorldOverrideSkeleton.SpineInstance.StateMachine != null)
        {
            WorldOverrideSkeleton.SpineInstance.StateMachine.SetBool("moving", moving);
        }

        if (SpineAnimator.Alive() && SpineAnimator.SpineInstance != null && SpineAnimator.SpineInstance.StateMachine != null)
        {
            SpineAnimator.SpineInstance.StateMachine.SetBool("sprinting", isSprinting);
        }

        // stamina
        if (isSprinting == true)
        {
            CurrentStamina -= 25 * Time.DeltaTime;
        }

        if (isSprinting == false && CurrentStamina < MaxStamina)
        {
            CurrentStamina += 20 * Time.DeltaTime;
        }

        if (CurrentStamina < MaxStamina)
        {
            DrawStaminaBar();
        }

        DrawPickupIcon();

        if (IsLocal)
        {
            if (ActionCam)
            {
                if (TryGetTeamAndBallBounds(out var groupCenter, out var _groupRadius))
                {
                    CurrentCamSlider = GameManager.SmoothDamp(CurrentCamSlider, 1f, ref CurrentCamVelocity, 0.4f);
                    Vector2 currentCamPos = Vector2.Lerp(CameraControl.Position, groupCenter, CurrentCamSlider);
                    CameraControl.Position = currentCamPos;
                }
                else if (SecondaryCamTarget.Alive())
                {
                    CurrentCamSlider = GameManager.SmoothDamp(CurrentCamSlider, 1f, ref CurrentCamVelocity, 0.4f);
                    Vector2 playerPos = this.Entity.Position + new Vector2(0, 0.5f);
                    Vector2 midpoint = Vector2.Lerp(playerPos, SecondaryCamTarget.Position, 0.5f);
                    Vector2 currentCamPos = Vector2.Lerp(CameraControl.Position, midpoint, CurrentCamSlider);
                    CameraControl.Position = currentCamPos;
                }
                else
                {
                    Vector2 playerPos = this.Entity.Position + new Vector2(0, 0.5f);
                    CurrentCamSlider = GameManager.SmoothDamp(CurrentCamSlider, 0f, ref CurrentCamVelocity, 0.4f);
                    Vector2 currentCamPos = Vector2.Lerp(playerPos, CameraControl.Position, CurrentCamSlider);
                    CameraControl.Position = currentCamPos;
                }
            }
            else
            {
                Vector2 playerPos = this.Entity.Position + new Vector2(0, 0.5f);
                CurrentCamSlider = GameManager.SmoothDamp(CurrentCamSlider, 0f, ref CurrentCamVelocity, 0.4f);
                Vector2 currentCamPos = Vector2.Lerp(playerPos, CameraControl.Position, CurrentCamSlider);
                CameraControl.Position = currentCamPos;
            }
        }

        if (CurrentItemInHand != oldItemInHand)
        {
            AdjustSkins();
            if (IsLocal)
            {
                CallServer_RemoveAllAimEffects(this);
            }
        }

        DashDecay();
    }

    [ServerRpc]
    public void RemoveAllAimEffects(MyPlayer player)
    {
        CallClient_CRemoveAllAimEffects(player);
    }

    [ClientRpc]
    public void CRemoveAllAimEffects(MyPlayer player)
    {
        foreach (AEffect effect in player.Effects)
        {
            if (effect is AimEffect)
            {
                player.RemoveEffect(effect, true);
                break;
            }
        }
    }

    public void ARemoveAllAimEffects(MyPlayer player)
    {
        foreach (AEffect effect in player.Effects)
        {
            if (effect is AimEffect)
            {
                player.RemoveEffect(effect, true);
                break;
            }
        }
    }

    string currentSkin = "";
    void AdjustSkins()
    {
        if (currentSkin != "")
        {
            SpineAnimator.SpineInstance.DisableSkin(currentSkin);
        }

        switch (CurrentItemInHand)
        {
            case "Assault Rifle":
                currentSkin = "weapons/assault_rifle";
                SpineAnimator.SpineInstance.EnableSkin("weapons/assault_rifle");
                break;

            case "Balloon Sword":
                currentSkin = "weapons/balloon_sword";
                SpineAnimator.SpineInstance.EnableSkin("weapons/balloon_sword");
                break;

            case "Baseball Bat":
                currentSkin = "weapons/baseball_bat";
                SpineAnimator.SpineInstance.EnableSkin("weapons/baseball_bat");
                break;

            case "Beam Saber":
                currentSkin = "weapons/beam_saber";
                SpineAnimator.SpineInstance.EnableSkin("weapons/beam_saber");
                break;

            case "Bee Cannon":
                currentSkin = "weapons/beehive_launcher";
                SpineAnimator.SpineInstance.EnableSkin("weapons/beehive_launcher");
                break;

            case "Chicken Sword":
                currentSkin = "weapons/chicken";
                SpineAnimator.SpineInstance.EnableSkin("weapons/chicken");
                break;

            case "Katana":
                currentSkin = "weapons/katana";
                SpineAnimator.SpineInstance.EnableSkin("weapons/katana");
                break;

            case "Raygun":
                currentSkin = "weapons/ray_gun";
                SpineAnimator.SpineInstance.EnableSkin("weapons/ray_gun");
                break;

            case "Rocket Launcher":
                currentSkin = "weapons/rocket_launcher";
                SpineAnimator.SpineInstance.EnableSkin("weapons/rocket_launcher");
                break;

            case "Shotgun":
                currentSkin = "weapons/shotgun";
                SpineAnimator.SpineInstance.EnableSkin("weapons/shotgun");
                break;

            case "Sniper Rifle":
                currentSkin = "weapons/sniper_rifle";
                SpineAnimator.SpineInstance.EnableSkin("weapons/sniper_rifle");
                break;

            case "Revolver":
                currentSkin = "weapons/revolver";
                SpineAnimator.SpineInstance.EnableSkin("weapons/revolver");
                break;

            default:
                break;
        }

        SpineAnimator.SpineInstance.RefreshSkins();
        oldItemInHand = CurrentItemInHand;
    }

    public override void WriteFrameData(AO.StreamWriter writer)
    {
        Util.Assert(Network.IsClient);
        writer.WriteString(_currentItemID);
        writer.Write(isSprinting);
    }
    public override void ReadFrameData(AO.StreamReader reader)
    {
        _currentItemID = reader.ReadString();
        isSprinting = reader.Read<bool>();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (Network.IsServer)
        {        
            if (!PlayerCorpse.Value.Alive() && GameManager.Instance.State == GameState.Round)
            {
                var playerCorpse = References.Instance.PlayerCorpsePrefab.Instantiate<PlayerCorpse>();
                playerCorpse.Entity.Name = $"{Name}_corpse";
                playerCorpse.Entity.Position = new Vector2(1000, 1000);
                playerCorpse.PlayerName = Name;
                playerCorpse.ColorIndex = ColorIndex;

                Network.Spawn(playerCorpse.Entity);
                playerCorpse.ForPlayer.Set(this.Entity);
                GameManager.Instance.AllPlayerCorpses.Add(playerCorpse.Entity);
                playerCorpse.CallClient_SetSkins(SpineAnimator.SpineInstance.GetSkins());

                PlayerCorpse.Set(playerCorpse.Entity);
            }

        }     

        if (!IsDead)
        {
            //DrawHealthBar();
        }

        if (this.IsLocal)
        {
            if (Store.Instance.ItemShopOpen == true &&
                StoreEntity.Alive())
            {
                if (Vector2.Distance(Entity.Position, StoreEntity.Position) >= 4f)
                {
                    Store.Instance.ItemShopOpen = false;
                }
            }

            DrawDamageNumber();
        }

        // Camera control for size altering effects
        /*
        if (Size altering effects)
        {
            SpineAnimator.SpineInstance.Scale = new Vector2(CScaleMod, CScaleMod);
            SpineAnimator.SpineInstance.Speed = 1f / CScaleMod;
            if (IsLocal && CurrentZoomLevel.Value == 1f &&
                !DoesEffectControlZoom())
            {
                CameraControl.Zoom = 1f + ((CScaleMod - 1f) * 0.33f);
            }
        }
        else
        */
        {
            SpineAnimator.SpineInstance.Scale = new Vector2(ScaleMod, ScaleMod);
            SpineAnimator.SpineInstance.Speed = 1f / ScaleMod;
            if (IsLocal && CurrentZoomLevel.Value == 1f &&
                !DoesEffectControlZoom())
            {
                CameraControl.Zoom = 1f + ((ScaleMod - 1f) * 0.33f);
            }
        }
    }

    bool DoesEffectControlZoom()
    {
       
        return false;
    }

    public void DrawArrowToPosition(Vector2 position, bool red)
    {
        // Copied from Fatsim sell area stuff
        var aspect = UI.SafeRect.Width / UI.SafeRect.Height;
        var worldOffset = position - Entity.Position;
        var targetPlayerScreenPos = Camera.WorldToScreen(position + new Vector2(0, 1f));
        var killerPlayerScreenPos = Camera.WorldToScreen(Entity.Position + new Vector2(0, 0.5f));
        var dir = (targetPlayerScreenPos - killerPlayerScreenPos).Normalized;
        var pos = killerPlayerScreenPos;
        var distance = worldOffset.Length;
        float arrowSize = 40;
        var anim = (float)Math.Pow(Math.Abs(Math.Sin(Math.PI * Time.TimeSinceStartup)), 0.75);
        float distanceThreshold = 5;
        distanceThreshold = ((dir * distanceThreshold) / new Vector2(1, aspect)).Length;
        if (distance >= (distanceThreshold + 0.5f))
        {
            var t = 1 - Ease.T(distance - distanceThreshold, 1);
            var arrowScreenPos = new Rect(pos, pos).Offset(dir.X * 300, dir.Y * 300).Center; // note(josh): using rects to scale by screen size
            arrowScreenPos = Vector2.Lerp(arrowScreenPos, targetPlayerScreenPos, t);
            var rect = new Rect(arrowScreenPos, arrowScreenPos).Grow(arrowSize);
            var rotation = Math.Atan2(dir.Y, dir.X) * (180.0 / Math.PI);
            UI.Image(rect, Assets.GetAsset<Texture>(red ? "RedArrow.png" : "BlueArrow.png"), new Vector4(1, 1, 1, 0.75f), default, (float)rotation);
        }
        else
        {
            var rect = new Rect(targetPlayerScreenPos, targetPlayerScreenPos).Grow(arrowSize);
            rect = rect.Offset(0, anim * 50);
            UI.Image(rect, Assets.GetAsset<Texture>(red ? "RedArrow.png" : "BlueArrow.png"), new Vector4(1, 1, 1, 0.75f), default, 270);
        }
    }

    public bool isHoldingSprint = false;
    public bool isSprinting = false;
    float ranOutOfStaminaTimer = 0;
    public float diveCooldown = 0;
    public float kickCooldown = 0;

    public void DrawSpecialButtons()
    {
        using var _ = UI.PUSH_LAYER(GameManager.PuzzleLayer);
        var rightRect = UI.SafeRect.BottomRightRect();

        // Kick
        if (!Game.IsMobile)
        {
            rightRect = rightRect.Offset(-110, 115).Grow(100);
            using var _1 = UI.PUSH_ID("KickButton");
            var bs = new UI.ButtonSettings();

            bs.Sprite = Assets.GetAsset<Texture>("AbilityIcons/KickIcon.png");


            // Grey out button if no stamina left
            if (kickCooldown > 0)
            {
                bs.BackgroundColorMultiplier = new Vector4(0.25f, 0.25f, 0.25f, 1f);
            }
            else
            {
                bs.BackgroundColorMultiplier = new Vector4(1f, 1f, 1f, 1f);
            }

            var ts = GameManager.Instance.GetTextSettings(28);
            ts.OverflowWrap = true;
            if (UI.BeginButton(rightRect.FitAspect(bs.Sprite.Aspect), kickCooldown > 0 ? Math.Round(kickCooldown).ToString() : "Kick", bs, ts).JustPressed || Input.GetMouseDown(Input.MouseButton.MOUSE_LEFT))
            {
                if (kickCooldown <= 0 && !HasEffect<AimEffect>())
                {
                    TryLocalSetCurrentTargettingAbility(GetAbility<AbilityKick>(), 0);
                }
            }

            UI.EndButton();

            if (kickCooldown > 0) kickCooldown -= Time.DeltaTime;

            // control image for pc players
            if (!Game.IsMobile)
            {
                var space = Assets.GetAsset<Texture>("$AO/ui/controls/mouse/lmb.png");
                UI.Image(rightRect.TopLeftRect().Offset(45, -15).Grow(25).FitAspect(space.Aspect), space);
            }
        }

        // Sprint 
        {
            rightRect = UI.SafeRect.BottomRightRect().Offset(-120, Game.IsMobile ? 375 : 315).Grow(80);

            using var _1 = UI.PUSH_ID("SprintButton");
            var bs = new UI.ButtonSettings();

            bs.Sprite = Assets.GetAsset<Texture>("AbilityIcons/SprintIcon.png");


            // Grey out button if no stamina left
            if (CurrentStamina <= 0 || ranOutOfStaminaTimer > 0)
            {
                bs.BackgroundColorMultiplier = new Vector4(0.25f, 0.25f, 0.25f, 1f);
            }
            else
            {
                bs.BackgroundColorMultiplier = new Vector4(1f, 1f, 1f, 1f);
            }

            var ts = GameManager.Instance.GetTextSettings(28);
            ts.OverflowWrap = true;
            if (UI.BeginButton(rightRect.FitAspect(bs.Sprite.Aspect), "Sprint", bs, ts).Pressed && this.Agent.Velocity.Length > 0.03f)
            {
                if (CurrentStamina > 0 && ranOutOfStaminaTimer <= 0)
                {
                    isHoldingSprint = true;
                }
                else
                {
                    isHoldingSprint = false;
                    if (ranOutOfStaminaTimer <= 0) ranOutOfStaminaTimer = 1f;
                }
            }
            else
            {
                isHoldingSprint = false;
            }

            UI.EndButton();

            if (!Game.IsMobile)
            {
                if ((Input.GetKeyHeld2(Input.Keycode.KEYCODE_LEFT_SHIFT) || Input.GetKeyHeld2(Input.Keycode.KEYCODE_RIGHT_SHIFT)) && this.Agent.Velocity.Length > 0.03f)
                {
                    if (CurrentStamina > 0 && ranOutOfStaminaTimer <= 0)
                    {
                        isHoldingSprint = true;
                    }
                    else
                    {
                        isHoldingSprint = false;
                        if (ranOutOfStaminaTimer <= 0) ranOutOfStaminaTimer = 1f;
                    }
                }
                else
                {
                    isHoldingSprint = false;
                }
            }

            if (isHoldingSprint && !isSprinting)
            {
                //CallServer_ToggleSprint(true);
                isSprinting = true;
            }
            else if (!isHoldingSprint && isSprinting)
            {
                //CallServer_ToggleSprint(false);
                isSprinting = false;
            }

            if (ranOutOfStaminaTimer > 0)
            {
                ranOutOfStaminaTimer -= Time.DeltaTime;
            }

            // control image for pc players
            if (!Game.IsMobile)
            {
                var shift = Assets.GetAsset<Texture>("shift.png");
                UI.Image(rightRect.TopLeftRect().Offset(25, -25).Grow(35).FitAspect(shift.Aspect), shift);
            }
        }

        // Dive
        if (!Game.IsMobile)
        {
            rightRect = UI.SafeRect.BottomRightRect().Offset(-300, 125).Grow(50);

            using var _1 = UI.PUSH_ID("DiveButton");
            var bs = new UI.ButtonSettings();

            bs.Sprite = Assets.GetAsset<Texture>("AbilityIcons/roll-icon.png");


            // Grey out button if no stamina left
            if (CurrentStamina <= 0 || ranOutOfStaminaTimer > 0)
            {
                bs.BackgroundColorMultiplier = new Vector4(0.25f, 0.25f, 0.25f, 1f);
            }
            else
            {
                bs.BackgroundColorMultiplier = new Vector4(1f, 1f, 1f, 1f);
            }

            var ts = GameManager.Instance.GetTextSettings(28);
            ts.OverflowWrap = true;
            if ((UI.BeginButton(rightRect.FitAspect(bs.Sprite.Aspect), "Dive", bs, ts).JustPressed || Input.GetKeyDown(Input.Keycode.KEYCODE_SPACE)) && this.Agent.Velocity.Length > 0.03f)
            {
                if (CurrentStamina > 0 && ranOutOfStaminaTimer <= 0)
                {
                    if (diveCooldown <= 0)
                    {
                        ActivateAbility<AbilityDodgeRoll>(2);
                    }
                }
                else
                {
                    // nothing
                }
            }          

            UI.EndButton();

            if (diveCooldown > 0) diveCooldown -= Time.DeltaTime;

            // control image for pc players
            if (!Game.IsMobile)
            {
                var img = Assets.GetAsset<Texture>("$AO/ui/controls/keyboard/space.png");
                UI.Image(rightRect.TopLeftRect().Offset(25, -25).Grow(30).FitAspect(img.Aspect), img);
            }
        }

        // Ability
        if (!Game.IsMobile)
        {
            if (PickedUpAbility.Value != 0)
            {
                rightRect = UI.SafeRect.BottomRightRect().Offset(-300, 300).Grow(50);

                using var _1 = UI.PUSH_ID("PickedUpAbilityButton");
                var bs = new UI.ButtonSettings();

                bs.Sprite = Assets.GetAsset<Texture>(GameData.GetAbilityIcon((AbilityPickups)PickedUpAbility.Value));

                var ts = GameManager.Instance.GetTextSettings(28);
                ts.OverflowWrap = true;
                if ((UI.BeginButton(rightRect.FitAspect(bs.Sprite.Aspect), GameData.GetAbilityName((AbilityPickups)PickedUpAbility.Value), bs, ts).JustPressed || Input.GetKeyDown(Input.Keycode.KEYCODE_R)))
                {
                    switch ((AbilityPickups)PickedUpAbility.Value)
                    {
                        case AbilityPickups.SuperKick:
                            TryLocalSetCurrentTargettingAbility(GetAbility<AbilitySuperKick>(), 0);
                            break;

                        case AbilityPickups.ExtraBall:
                            ActivateAbility<AbilityExtraBall>(0);
                            break;

                        case AbilityPickups.WarpStep:
                            TryLocalSetCurrentTargettingAbility(GetAbility<AbilityWarpStep>(), 0);
                            break;

                        case AbilityPickups.Bomb:
                            TryLocalSetCurrentTargettingAbility(GetAbility<AbilityBomb>(), 0);
                            break;
                    }
                }

                UI.EndButton();

                // control image for pc players
                if (!Game.IsMobile)
                {
                    var img = Assets.GetAsset<Texture>("$AO/ui/controls/keyboard/R.png");
                    UI.Image(rightRect.TopLeftRect().Offset(25, -25).Grow(15).FitAspect(img.Aspect), img);
                }
            }
        }

    }

    [ServerRpc]
    public void ToggleSprint(bool sprinting)
    {
        MyPlayer player = (MyPlayer)Network.GetRemoteCallContextPlayer();
        Sprinting.Set(sprinting);
    }

    public void DrawReadyGo()
    {
        {
            using var _ = UI.PUSH_LAYER(GameManager.PuzzleLayer);
            var fullScreenRect = UI.ScreenRect.CenterRect().Grow(960, 540, 960, 540);
            ReadyGo.Update(Time.DeltaTime);
            UI.DrawSkeleton(fullScreenRect, ReadyGo, new Vector2(200, 200), 0);
        }
    }

    public UI.TextSettings GetTextSettings(float size, float offset = 0, FontAsset font = null, UI.HorizontalAlignment halign = UI.HorizontalAlignment.Center)
    {
        if (font == null)
        {
            font = UI.Fonts.BarlowBold;
        }
        var ts = new UI.TextSettings()
        {
            Font = font,
            Size = size,
            Color = Vector4.White,
            DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
            DropShadowOffset = new Vector2(0f, -3f),
            HorizontalAlignment = halign,
            VerticalAlignment = UI.VerticalAlignment.Center,
            WordWrap = false,
            WordWrapOffset = 0,
            Outline = true,
            OutlineThickness = 3,
            Offset = new Vector2(0, offset),
        };
        return ts;
    }

    protected void DashDecay()
    {
        if (DashRemainingDuration > 0) DashRemainingDuration -= Time.DeltaTime;
        Dash = DashRemainingDuration > 0 ? Dash : Vector2.Zero;
    }

    [ClientRpc]
    public void AddDash(Vector2 add, float duration)
    {
        SetFacingDirection(add.X > 0);
        Dash = add;
        DashRemainingDuration = duration;
    }

    [ClientRpc]
    public void ShakeScreen(float intensity, float duration)
    {
        if (CameraControl != null)
        {
            CameraControl.Shake(intensity, duration);
        }
    }

    public bool TryRemoveItems(Item_Definition defn, int countToRemove)
    {
        var items = DefaultInventory.Items;
        int haveCount = 0;
        for (int i = items.Length - 1; i >= 0; i--)
        {
            var item = items[i];
            if (item == null) continue;
            if (item.Definition == defn)
            {
                haveCount += (int)item.Quantity;
            }
        }

        if (haveCount < countToRemove)
        {
            return false;
        }

        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (countToRemove == 0)
            {
                break;
            }

            var item = items[i];
            if (item == null) continue;
            if (item.Definition == defn)
            {
                if ((int)item.Quantity <= countToRemove)
                {
                    countToRemove -= (int)item.Quantity;
                    Inventory.DestroyItem(item);
                }
                else
                {
                    item.Quantity -= (long)countToRemove;
                    countToRemove = 0;
                    break;
                }
            }
        }

        return true;
    }

    public bool TryRemoveItems(string itemID, int countToRemove)
    {
        var defn = ItemManager.Instance.TryFindItem(itemID);
        if (defn == null) return false;
        return TryRemoveItems(defn, countToRemove);
    }

    [ClientRpc]
    public void SetFlash(Vector4 _flashColor)
    {
        FlashAlpha = 1f;
        FlashColor = _flashColor;
    }

    Ability AssignedMainAbility()
    {
        return GetAbility<AbilityKick>();
        return null;
    }

    Ability AssignedSecondaryAbility()
    {

        return GetAbility<AbilityDodgeRoll>();
    }

    Ability AssignedThirdAbility()
    {
        switch ((AbilityPickups)PickedUpAbility.Value)
        {
            case AbilityPickups.None: return null;
            case AbilityPickups.SuperKick: return GetAbility<AbilitySuperKick>();
            case AbilityPickups.ExtraBall: return GetAbility<AbilityExtraBall>();
                case AbilityPickups.WarpStep: return GetAbility<AbilityWarpStep>();
            case AbilityPickups.Bomb: return GetAbility<AbilityBomb>();
        }

        return null;
    }

    public void DrawAbilities()
    {
        if (IsLocal && HideHudReasons.Count == 0)
        {
            if (PlayerRole == PlayerRole.RedTeam)
            {
                DrawDefaultAbilityUI(new AbilityDrawOptions()
                {
                    AbilityElementSize = 200,
                    Abilities = new Ability[]{
                        Game.IsMobile ? AssignedMainAbility() : null,
                        Game.IsMobile ? AssignedSecondaryAbility() : null,
                        Game.IsMobile ? AssignedThirdAbility() : null,
                        null,
                        null,
                        null,
                    }
                });
            }

            if (PlayerRole == PlayerRole.BlueTeam)
            {
                DrawDefaultAbilityUI(new AbilityDrawOptions()
                {
                    AbilityElementSize = 200,
                    Abilities = new Ability[]{
                       Game.IsMobile ? AssignedMainAbility() : null,
                        Game.IsMobile ? AssignedSecondaryAbility() : null,
                        Game.IsMobile ? AssignedThirdAbility() : null,
                        null,
                        null,
                        null,
                    }
                });
            }

            if (PlayerRole == PlayerRole.Spectator)
            {
                DrawDefaultAbilityUI(new AbilityDrawOptions()
                {
                    AbilityElementSize = 200,
                    Abilities = new Ability[]{
                        null,
                        null,
                        null,
                        null,
                        null,
                    }
                });
            }
        }
    }

    public static T TryGetClosestComponent<T>(Vector2 point, float range = float.MaxValue) where T : Component
    {
        T closest = null;
        foreach (var t in Scene.Components<T>())
        {
            var distance = (t.Position - point).Length;
            if (distance < range)
            {
                range = distance;
                closest = t;
            }
        }
        return closest;
    }

    // INVENTORY
    public Item_Instance TryGetItem(string itemId)
    {
        var items = DefaultInventory.Items;
        foreach (Item_Instance item in items)
        {
            if (item != null && item.Definition.Id == itemId)
            {
                return item;
            }
        }
        return null;
    }

    public void ClearInventory()
    {
        var items = DefaultInventory.Items;
        foreach (var item in items)
        {
            if (item != null)
            {
                Inventory.DestroyItem(item);
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        ClearInventory();
        if (IsLocal)
        {
            CameraControl.Destroy();
        }
    }

    public bool AddItemToInventory(string itemId, long count = 1, float durability = 1f)
    {
        if (DefaultInventory == null)
        {
            return false;
        }
        var defn = ItemManager.Instance.TryFindItem(itemId);
        if (defn == null)
        {
            return false;
        }

        var itemsToDrop = new List<Item_Instance>();
        while (count > 0)
        {
            var stackSize = (long)defn.StackSize;
            if (stackSize > count) stackSize = count;
            count -= stackSize;

            var instance = ItemManager.CreateItem(defn, stackSize);

            if (instance.Durability == 1f)
            {
                instance.Durability = durability;
            }

            if (!Inventory.CanMoveItemToInventory(instance, DefaultInventory, out bool wdt))
            {
                itemsToDrop.Add(instance);
            }
            else
            {
                Inventory.MoveItemToInventory(instance, DefaultInventory);
            }
        }

        foreach (var item in itemsToDrop)
        {
            ServerDropItems(item);
        }

        return true;
    }

    /// <summary>
    /// A little check for puzzles or anything that needs to check if you have X of a specific item
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="reqAmount"></param>
    /// <returns></returns>
    public long GetItemAmount(string itemId)
    {
        long total = 0;
        foreach (var item in DefaultInventory.Items)
        {
            if (item != null && item.Definition.Id == itemId)
            {
                total += item.Quantity;
            }
        }
        return total;
    }

    public int InventoryBoxesSpawned;

    public void ServerDropItems(Item_Instance itemToDrop)
    {
        Util.Assert(Network.IsServer);

        ulong rng = RNG.Seed((ulong)Game.FrameNumber);
        var item = Network.InstantiateAndSpawn(References.Instance.ItemDropPrefab, e => { 
            e.Position = Entity.Position + new Vector2(RNG.RangeFloat(ref rng, -0.55f, 0.55f), RNG.RangeFloat(ref rng, -0.55f, 0.55f));
            e.GetComponent<ItemDrop>().CreateItemDrop(itemToDrop.Definition.Id, (int)itemToDrop.Quantity);
            e.GetComponent<ItemDrop>().timer = -0.5f;
            e.GetComponent<ItemDrop>().Durability = itemToDrop.Durability;
        });
    }

    public void ServerResetPlayer(PlayerRole role)
    {
        PlayerRole = role;
        CurrentHealth = 100;

        var playerCorpse = References.Instance.PlayerCorpsePrefab.Instantiate<PlayerCorpse>();
        playerCorpse.Entity.Name = $"{Name}_corpse";
        playerCorpse.Entity.Position = new Vector2(1000, 1000);
        playerCorpse.PlayerName = Name;
        playerCorpse.ColorIndex = ColorIndex;

        Network.Spawn(playerCorpse.Entity);
        playerCorpse.ForPlayer.Set(this.Entity);
        GameManager.Instance.AllPlayerCorpses.Add(playerCorpse.Entity);
        playerCorpse.CallClient_SetSkins(SpineAnimator.SpineInstance.GetSkins());

        PlayerCorpse.Set(playerCorpse.Entity);
      
        TimeIdleInSeconds = 0;
    }

    public Item_Instance HasItem(string itemID, int amount = 1)
    {
        Item_Instance item = null;
        foreach (var i in DefaultInventory.Items)
        {
            if (i == null) continue;
            if (i.Definition.Id == itemID)
            {
                if (i.Quantity >= amount)
                {
                    item = i;
                    break;
                }
            }
        }
        return item;
    }

    [ClientRpc]
    public void ResetPlayer(int role)
    {
        SpineAnimator.SpineInstance.DisableSkin("RedTeam");
        SpineAnimator.SpineInstance.DisableSkin("BlueTeam");
        // Set My team skin
        SpineAnimator.SpineInstance.EnableSkin(role == (int)PlayerRole.RedTeam ? "RedTeam" : "BlueTeam");
        SpineAnimator.SpineInstance.RefreshSkins();
    }


    [ClientRpc]
    public void ShowNotificationLocal(string message)
    {
        if (IsLocal)
        {
            Notifications.Show(message);
        }
    }

    [ClientRpc]
    public void KillPlayer(Player player)
    {
        MyPlayer p = (MyPlayer)player;
        p.ClearAllEffects();
        //p.AddEffect<EffectDeath>();
    }

    [ClientRpc]
    public void PlayShopSFX(Player player)
    {
        if (player.IsLocal)
        {
            SFXE.Play(Assets.GetAsset<AudioAsset>("sfx/PurchaseSFX.wav"), new() { });
        }
    }

    [ServerRpc]
    public void SetIsMobile(bool mobile)
    {
        MyPlayer p = (MyPlayer)Network.GetRemoteCallContextPlayer();
        p.Mobile.Set(mobile);
    }

    public bool DrawCloseButton()
    {
        var exitButton = Assets.GetAsset<Texture>("fail_x.png");
        var exitButtonRect = UI.ScreenRect.TopRightRect().Grow(0, 0, 100, 100).FitAspect(exitButton.Aspect).Offset(-200, -100);
        if (UI.Button(exitButtonRect, "EXIT", new UI.ButtonSettings() { Sprite = exitButton }, new UI.TextSettings()).JustPressed || Input.GetKeyDown(Input.Keycode.KEYCODE_ESCAPE, true))
        {
            return true;
        }
        return false;
    }

    // @Credit: Lookumz
    protected Rect DrawStaminaBar()
    {
        using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
        using var _2 = IM.PUSH_Z(GetZOffset() - 0.0001f); // minus an epsilon so the health bar draws over the player
        using var _3 = UI.PUSH_SCALE_FACTOR(5.0f / 540.0f);
        var healthRect = FinalNameRect.BottomCenterRect().Offset(0, -200);
        healthRect = healthRect.Grow(13, 70, 0, 70).Offset(0, 0);
        var borderRect = healthRect.Grow(5.5f, 4, 5.5f, 4).Offset(0, -2);

        var back = StamBarBack;
        var fill = StamBarFill;

        // Draw bar background
        UI.Image(borderRect, back, Vector4.White, new UI.NineSlice());

        // Draw health percentage
        var healthPercent = CurrentStamina / MaxStamina;
        var healthPercentRect = healthRect.SubRect(0, 0, healthPercent, 1, 0, 0, 0, 0);
        UI.Image(healthPercentRect, fill, Vector4.White, new UI.NineSlice());
      
        return healthRect;
    }

    public void DrawPickupIcon()
    {
        using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
        using var _2 = IM.PUSH_Z(GetZOffset() - 0.0001f); // minus an epsilon so the health bar draws over the player
        using var _3 = UI.PUSH_SCALE_FACTOR(5.0f / 540.0f);
        var pickupRect = this.FinalNameRect.CenterRect().Offset(0, 45);
        if (this.PickedUpAbility.Value != 0)
        {
            var tex = Assets.GetAsset<Texture>(GameData.GetAbilityIcon((AbilityPickups)PickedUpAbility.Value));
            UI.Image(pickupRect.Grow(30).FitAspect(tex.Aspect), tex);
        }
    }

    void DrawCamButton()
    {
        using var _ = UI.PUSH_LAYER(GameManager.PuzzleLayer);
        {
            var leftRect = UI.SafeRect.LeftCenterRect();
            leftRect = leftRect.Offset(Game.IsMobile ? 135 : 135, 55).Grow(50, 135, 50, 135);

            using var _1 = UI.PUSH_ID("CamToggle");
            var buttonRect1 = leftRect.BottomLeftRect().Offset(55, -75).Grow(40);
            var bs = new UI.ButtonSettings();
            bs.Sprite = Assets.GetAsset<Texture>(ActionCam ? "AbilityIcons/CamFocus.png" : "AbilityIcons/CamBall.png");
            bs.BackgroundColorMultiplier = new Vector4(1f, 1f, 1f, 1f);

            var ts = GameManager.Instance.GetTextSettings(28);
            if (UI.BeginButton(buttonRect1.FitAspect(bs.Sprite.Aspect), "Mode", bs, ts).JustPressed)
            {
                ActionCam = !ActionCam;
            }

            UI.EndButton();
        }
    }

    protected void DrawDamageNumber()
    {
        using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
        using var _2 = UI.PUSH_LAYER(5);

        List<DamageNumbers> numbers = GameManager.Instance.ActiveDamageNumbers;
        for (int i = numbers.Count - 1; i >= 0; i -= 1)
        {
            var result = numbers[i];
            float speed = 0.5f;
            var ts = result.TextSettings;

            result.T += Time.DeltaTime * speed;
            if (result.T >= 1 && result.DoingFading)
            {
                numbers.UnorderedRemoveAt(i);
                continue;
            }
            if (result.T >= 1 && !result.DoingFading)
            {
                result.T = 0.0f;
                result.DoingFading = true;
            }

            if (!result.DoingFading)
            {
                var pos = result.Position;
                pos.Y += AOMath.Lerp(0, 0.5f, Ease.OutQuart(result.T));               
                var color01 = Ease.FadeInAndOut(0.1f, 1f, result.T);
                ts.Color = Vector4.Lerp(ts.Color, result.Color, color01);
                result.LastPosition = pos;
            }
            else
            {
                ts.SpacingMultiplier = 1f;
                var colorAlpha = Vector4.Zero;
                ts.Color = Vector4.Lerp(ts.Color, colorAlpha, result.T);
            }

            var rect = new Rect(result.LastPosition, result.LastPosition);
            UI.TextAsync(rect, result.Text, ts);
        }
    }

    [ClientRpc]
    public void NotifyPlayer(string notif)
    {
        if (!IsLocal) return;
        Notifications.Show(notif);
    }

    public PlayerCorpse PlaceCorpseAndDropItems()
    {
        var corpse = PlayerCorpse.Value.GetComponent<PlayerCorpse>();
        corpse.Entity.Position = Entity.Position;

        corpse.PlayerAnimator.SpineInstance.StateMachine.SetTrigger("die");
        corpse.DeathAnim = "die";
        SFXE.Play(Assets.GetAsset<AudioAsset>("sfx/0hp_death.wav"), new() { Positional = true, Position = Entity.Position });

        return corpse;
    }

    [ServerRpc]
    public void PleaseDropItem(long itemIndex)
    {
        var items = DefaultInventory.Items;
        if (itemIndex < 0 || itemIndex >= items.Length) return;
        var player = (MyPlayer)Network.GetRemoteCallContextPlayer();
        if (!player.Alive()) return;
        if (player.HasActiveEffect) return;
        var item = items[itemIndex];
        if (item == null) return;
        if (item.Inventory != player.DefaultInventory) return;
        
        ServerDropItems(item);
        TryRemoveItems(item.Definition, (int)item.Quantity);
    }

    public static void DrawTVEffect()
    {
        {
            using var _ = IM.PUSH_MATERIAL(GameManager.Instance.StaticMaterial);
            UI.Image(UI.ScreenRect, UI.WhiteSprite, new Vector4(1, 1, 1, 0.5f));
        }
        UI.Image(UI.ScreenRect, Assets.GetAsset<Texture>("crt_frame.png"));
    }

}

public enum PlayerRole
{
    Spectator = 0,
    RedTeam,
    BlueTeam
}

#region Effects
public abstract class MyEffect : AEffect
{
    public new MyPlayer Player => (MyPlayer)base.Player;
}

public abstract class AimEffect : MyEffect {}

public class SpectatorEffect : MyEffect
{
    public override bool IsActiveEffect => false;
    public override bool IsValidTarget => false;

    public bool HasNameInvisReason = false;

    public void UpdateInvis()
    {
        if (Player.IsLocal || (Network.LocalPlayer.Alive() && Network.LocalPlayer.HasEffect<SpectatorEffect>()))
        {
            Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 0.5f);
            if (HasNameInvisReason)
            {
                HasNameInvisReason = false;
                Player.RemoveNameInvisibilityReason(nameof(SpectatorEffect));
            }
        }
        else
        {
            Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 0);
            if (!HasNameInvisReason)
            {
                HasNameInvisReason = true;
                Player.AddNameInvisibilityReason(nameof(SpectatorEffect));
            }
        }
    }

    public override void OnEffectStart(bool isDropIn)
    {
       // UpdateInvis();
        Player.SpineAnimator.DepthOffset = -10000;
        Player.SpineAnimator.SpineInstance.StateMachine.SetBool("ghost_form", true);
        Player.Entity.GetComponent<Circle_Collider>().LocalEnabled = false;
        if (!isDropIn)
        {
            Player.AddEmoteBlockReason(nameof(SpectatorEffect));
        }

        if (Player.IsLocal && GameManager.Instance.VoiceChatEnabled)
        {
            Game.SetVoiceEnabled(false);
        }
    }

    public override void OnEffectUpdate()
    {
        //UpdateInvis();
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.RemoveEmoteBlockReason(nameof(SpectatorEffect));
        Player.SpineAnimator.DepthOffset = 0;
        Player.SpineAnimator.SpineInstance.StateMachine.SetBool("ghost_form", false);
        Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 1);
        if (HasNameInvisReason)
        {
            Player.RemoveNameInvisibilityReason(nameof(SpectatorEffect));
        }
        Player.Entity.GetComponent<Circle_Collider>().LocalEnabled = true;

        if (Player.IsLocal && GameManager.Instance.VoiceChatEnabled)
        {
            Game.SetVoiceEnabled(true);
        }
        Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 1);
    }
}

public class NoClipEffect : MyEffect
{
    public override bool IsActiveEffect => false;
    public override bool IsValidTarget => false;

    public override void OnEffectStart(bool isDropIn)
    {
        Player.Entity.GetComponent<Circle_Collider>().LocalEnabled = false;
    }

    public override void OnEffectUpdate()
    {
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.Entity.GetComponent<Circle_Collider>().LocalEnabled = true;
    }
}

public class WaitForAnimEffect : MyEffect
{
    public override bool IsActiveEffect => true;
    public override bool FreezePlayer => true;

    public override void OnEffectStart(bool isDropIn)
    {
        if (!isDropIn)
        {
            DurationRemaining = Player.SpineAnimator.SpineInstance.StateMachine.TryGetLayerByIndex(0).GetCurrentStateLength();
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
    }

    public override void OnEffectUpdate()
    {
    }
}

public abstract class MyAbility : Ability
{
    public new MyPlayer Player => (MyPlayer)base.Player;

    public override bool CanTarget(Player p)
    {
        var player = (MyPlayer)p;
        if (player.PlayerRole == PlayerRole.Spectator)
        {
            return false;
        }
        return true;
    }

    public override bool CanUse()
    {
        if (GameManager.Instance.GlobalAbilityBlocker)
        {
            return false;
        }
        if (GameManager.Instance.State != GameState.Round)
        {
            return false;
        }
        if (Player.PlayerRole == PlayerRole.Spectator)
        {
            return false;
        }
        return true;
    }
}


#endregion
