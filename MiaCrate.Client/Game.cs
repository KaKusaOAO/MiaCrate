using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using MiaCrate.Auth;
using MiaCrate.Auth.Yggdrasil;
using MiaCrate.Client.Colors;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Client.Multiplayer;
using MiaCrate.Client.Pipeline;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Realms;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Sounds;
using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using MiaCrate.Client.Utils;
using MiaCrate.Common;
using MiaCrate.Data;
using MiaCrate.DataFixes;
using MiaCrate.Resources;
using MiaCrate.Texts;
using MiaCrate.World.Entities;
using Mochi.Texts;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using ClientPackSource = MiaCrate.Client.Resources.ClientPackSource;

namespace MiaCrate.Client;

public class Game : ReentrantBlockableEventLoop<IRunnable>
{
	public static Game Instance { get; private set; } = null!;
    public static readonly bool OnMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private const int MaxTicksPerUpdate = SharedConstants.TicksPerSecond / 2; // Redstone ticks
    public static ResourceLocation DefaultFont { get; } = new("default");
    public static ResourceLocation UniformFont { get; } = new("uniform");
    public static ResourceLocation AltFont { get;  } = new("alt");
    private static readonly ResourceLocation _regionalCompliances = new("regional_compliances.json");
    private static readonly Task _resourceReloadInitialTask = Task.CompletedTask;
    private static readonly IComponent _socialInteractionsNotAvailable = TranslateText.Of("multiplayer.socialInteractions.not_available");
    public const string UpdateDriversAdvice = "Please make sure you have up-to-date drivers (see aka.ms/mcdriver for instructions).";
    private readonly string _resourcePackDirectory;
    private readonly PropertyMap _profileProperties;
    private readonly IDataFixer _fixerUpper;
    private readonly VirtualScreen _virtualScreen;
    public Window Window { get; }

    private readonly Timer _timer = new(SharedConstants.TicksPerSecond, 0L);
    private readonly RenderBuffers _renderBuffers;
    public LevelRenderer LevelRenderer { get; }
	public EntityRenderDispatcher EntityRenderDispatcher { get; }

	public ItemRenderer ItemRenderer { get; }

	// public readonly ParticleEngine particleEngine;
	// private readonly SearchRegistry _searchRegistry = new SearchRegistry();
	public User User { get; }
	public Font Font { get; }
	public Font FontFilterFishy { get; }

	public GameRenderer GameRenderer { get; }

	// public readonly DebugRenderer debugRenderer;
	// private readonly AtomicReference<StoringChunkProgressListener> _progressListener = new AtomicReference();
	// public readonly Gui gui;
	public Options Options { get; }
	// private readonly HotbarManager _hotbarManager;
	public MouseHandler MouseHandler { get; }
	public KeyboardHandler KeyboardHandler { get; }
	private InputType _lastInputType = InputType.None;
	public string GameDirectory { get; }
	private readonly string _launchedVersion;
	private readonly string _versionType;
	private readonly IWebProxy _proxy;
	// private readonly LevelStorageSource _levelSource;
	// public readonly FrameTimer frameTimer;
	private readonly bool _is64Bit;
	private readonly bool _demo;
	private readonly bool _allowsMultiplayer;
	private readonly bool _allowsChat;
	public ReloadableResourceManager ResourceManager { get; }

	// private readonly DownloadedPackSource _downloadedPackSource;
	private readonly PackRepository _resourcePackRepository;
	public LanguageManager LanguageManager { get; }
	private readonly BlockColors _blockColors;
	private readonly ItemColors _itemColors;
	public RenderTarget MainRenderTarget { get; }

	public SoundManager SoundManager { get; }
	// private readonly MusicManager _musicManager;
	private readonly FontManager _fontManager;
	// private readonly SplashManager _splashManager;
	// private readonly GpuWarnlistManager _gpuWarnlistManager;
	// private readonly PeriodicNotificationManager _regionalCompliancies;
	private readonly YggdrasilAuthenticationService _authenticationService;
	private readonly IMinecraftSessionService _minecraftSessionService;
	// private readonly UserApiService _userApiService;
	// private readonly SkinManager _skinManager;
	public ModelManager ModelManager { get; }

	public BlockRenderDispatcher BlockRenderer { get; }

	// private readonly PaintingTextureManager _paintingTextures;
	// private readonly MobEffectTextureManager _mobEffectTextures;
	// private readonly ToastComponent _toast;
	// private readonly Tutorial _tutorial;
	// private readonly PlayerSocialManager _playerSocialManager;
	private readonly EntityModelSet _entityModels;
	private readonly BlockEntityRenderDispatcher _blockEntityRenderDispatcher;
	// private readonly ClientTelemetryManager _telemetryManager;
	// private readonly ProfileKeyPairManager _profileKeyPairManager;
	// private readonly RealmsDataFetcher _realmsDataFetcher;
	// private readonly QuickPlayLog _quickPlayLog;
	// public MultiPlayerGameMode gameMode;
	public ClientLevel? Level { get; set; }
	// public LocalPlayer player;
	// private IntegratedServer _singleplayerServer;
	// private Connection _pendingConnection;
	private bool _isLocalServer;
	public Entity cameraEntity;
	public Entity? crosshairPickEntity;
	// public HitResult? hitResult;
	private int _rightClickDelay;
	public int missTime;
	private bool _pause;
	private float _pausePartialTick;
	private long _lastNanoTime = Util.GetNanos();
	private long _lastTime;
	private int _frames;
	public bool NoRender { get; set; }

	public Screen? Screen
	{
		get => _screen;
		set
		{
			_screen?.Removed();
			if (value == null && Level == null)
			{
				value = new TitleScreen();
			}
			else
			{
				Util.LogFoobar();
			}

			_screen = value;
			_screen?.Added();
			
			BufferUploader.Reset();

			if (value != null)
			{
				MouseHandler.ReleaseMouse();
				KeyMapping.ReleaseAll();
				value.Init(this, Window.GuiScaledWidth, Window.GuiScaledHeight);
				NoRender = false;
			}
			
			UpdateTitle();
		}
	}

	public Overlay? Overlay { get; set; }
	private bool _connectedToRealms;
	private Thread _gameThread;
	private Func<CrashReport>? _delayedCrash;
	private static int _fps;
	public string FpsString { get; private set; } = "";
	private long _frameTimeNs;
	public bool wireframe;
	public bool chunkPath;
	public bool chunkVisibility;
	public bool SmartCull { get; set; } = true;
	private bool _windowActive;
	// private TutorialToast _socialInteractionsToast;
	private IProfilerFiller _profiler;
	private int _fpsPieRenderTicks;
	// private readonly ContinuousProfiler _fpsPieProfiler;
	private IProfileResults? _fpsPieResults;
	// private MetricsRecorder _metricsRecorder;
	private readonly ResourceLoadStateTracker _reloadStateTracker = new();
	private long _savedCpuDuration;
	private double _gpuUtilization;
	// private TimerQuery.FrameProfile? _currentFrameProfile;
	// private readonly Realms32BitWarningStatus _realms32BitWarningStatus;
	// private readonly GameNarrator _narrator;
	// private readonly ChatListener _chatListener;
	// private ReportingContext _reportingContext;
	private string _debugPath = "root";
    private readonly ConcurrentQueue<Action> _progressTasks = new();
    private TaskCompletionSource? _pendingReload;
    public bool IsGameLoadFinished { get; private set; }
    private Screen? _screen;

    public GuiSpriteManager GuiSprites { get; }
    public VanillaPackResources VanillaPackResources { get; }
    public TextureManager TextureManager { get; }
    public bool IsWindowActive { get; set; }

    public bool IsRenderedOnThread => SharedConstants.MultiThreadedRendering;
    public bool IsRunning { get; private set; }

    public Game(GameConfig config) : base("Client")
    {
	    LastInputType = InputType.None;
	    Instance = this;
        GameDirectory = config.Location.GameDirectory;
        _resourcePackDirectory = config.Location.ResourcePackDirectory;
        _launchedVersion = config.Game.LaunchVersion;
        _versionType = config.Game.VersionType;
        _profileProperties = config.User.ProfileProperties;
        
        var clientPackSource = new ClientPackSource(config.Location.ExternalAssetSource);
        // var repositorySource = new FolderRepositorySource(_resourcePackDirectory, PackType.ClientResources, IPackSource.Default);

        var modPackSource = new ModPackSource();
        _resourcePackRepository = new PackRepository(clientPackSource, modPackSource);
        VanillaPackResources = clientPackSource.VanillaPack;
        _proxy = config.User.Proxy;
        _authenticationService = new YggdrasilAuthenticationService(_proxy);
        // _minecraftSessionService = _authenticationService.CreateMinecraftSessionService();
        User = config.User.User;
        Logger.Info($"Setting user: {User.Name}");
        Logger.Verbose($"(Session ID is {User.SessionId})");

        _demo = config.Game.IsDemo;
        _allowsMultiplayer = !config.Game.IsMultiplayerDisabled;
        _allowsChat = !config.Game.IsChatDisabled;
        _fixerUpper = DataFixers.DataFixer;
        _gameThread = Thread.CurrentThread;
        Options = new Options(this, GameDirectory); 
        IsRunning = true;
        Logger.Info($"Backend library: {RenderSystem.GetBackendDescription()}");

        var displayData = config.Display;
        Util.TimeSource = RenderSystem.InitBackendSystem();
        _virtualScreen = new VirtualScreen(this);
        Window = _virtualScreen.NewWindow(displayData, null, CreateTitle());
        Window.WindowActiveChanged += WindowOnWindowActiveChanged;
        Window.DisplayResized += WindowOnDisplayResized;
        Window.CursorEntered += WindowOnCursorEntered;
        IsWindowActive = true;

        MouseHandler = new MouseHandler(this);
        KeyboardHandler = new KeyboardHandler(this);

        unsafe
        {
	        MouseHandler.Setup(Window.Handle);
	        KeyboardHandler.Setup(Window.Handle);
        }

        RenderSystem.InitRenderer(0, false);
        
        // Initialize the main render target
        MainRenderTarget = new MainTarget(Window.Width, Window.Height);
        MainRenderTarget.SetClearColor(0f, 0f, 0f, 0f);
        MainRenderTarget.Clear(OnMacOs);
        
        ResourceManager = new ReloadableResourceManager(PackType.ClientResources);
        
	    // Find all the available packs and apply vanilla pack if not
        _resourcePackRepository.Reload();

        LanguageManager = new LanguageManager();
        ResourceManager.RegisterReloadListener(LanguageManager);

        TextureManager = new TextureManager(ResourceManager);
        ResourceManager.RegisterReloadListener(TextureManager);
        
        SoundManager = new SoundManager(Options);
        ResourceManager.RegisterReloadListener(SoundManager);
        
        _fontManager = new FontManager(TextureManager);
        Font = _fontManager.CreateFont();
        FontFilterFishy = _fontManager.CreateFontFilterFishy();
        ResourceManager.RegisterReloadListener(_fontManager);

        Window.SetErrorSection("Startup");
        Window.SetErrorSection("Post startup");

        _blockColors = BlockColors.CreateDefault();
        _itemColors = ItemColors.CreateDefault();
        
        ModelManager = new ModelManager(TextureManager, _blockColors, 0); // options.mipmapLevels
        ResourceManager.RegisterReloadListener(ModelManager);

        _entityModels = new EntityModelSet();
        ResourceManager.RegisterReloadListener(_entityModels);

        _blockEntityRenderDispatcher = new BlockEntityRenderDispatcher(Font, _entityModels, () => BlockRenderer!,
	        () => ItemRenderer!, () => EntityRenderDispatcher!);
        ResourceManager.RegisterReloadListener(_blockEntityRenderDispatcher);

        var blockEntityRenderer = new BlockEntityWithoutLevelRenderer(_blockEntityRenderDispatcher, _entityModels);
        ResourceManager.RegisterReloadListener(blockEntityRenderer);

        ItemRenderer = new ItemRenderer(this, TextureManager, ModelManager, _itemColors, blockEntityRenderer);
        ResourceManager.RegisterReloadListener(ItemRenderer);
        
        _renderBuffers = new RenderBuffers();
        
        BlockRenderer = new BlockRenderDispatcher(ModelManager.BlockModelShaper, blockEntityRenderer, _blockColors);
        ResourceManager.RegisterReloadListener(BlockRenderer);

        EntityRenderDispatcher = new EntityRenderDispatcher(this, TextureManager, ItemRenderer, BlockRenderer, Font, Options, _entityModels);
        ResourceManager.RegisterReloadListener(EntityRenderDispatcher);
        
        GameRenderer = new GameRenderer(this, EntityRenderDispatcher.ItemInHandRenderer, ResourceManager, _renderBuffers);
        ResourceManager.RegisterReloadListener(GameRenderer.CreateReloadListener());

        LevelRenderer = new LevelRenderer(this, EntityRenderDispatcher, _blockEntityRenderDispatcher, _renderBuffers);
        ResourceManager.RegisterReloadListener(LevelRenderer);
        
        // CreateSearchTrees();

        GuiSprites = new GuiSpriteManager(TextureManager);
        ResourceManager.RegisterReloadListener(GuiSprites);

        var realmsClient = RealmsClient.Create(this);
        
        // RenderSystem.SetErrorCallback(...);

        WindowOnDisplayResized();
        GameRenderer.PreloadUiShader(VanillaPackResources.AsProvider());
        
        LoadingOverlay.RegisterTextures(this);
        var list = _resourcePackRepository.OpenAllSelected();
        _reloadStateTracker.StartReload(ResourceLoadStateTracker.ReloadReason.Initial, list);
        
        var cookie = new GameLoadCookie(realmsClient, config.QuickPlay);
  
        // Create a new reload instance and set overlay
        var reloadInstance = ResourceManager
	        .CreateReload(Util.BackgroundExecutor, this, _resourceReloadInitialTask, list);
        
        Overlay = new LoadingOverlay(this, reloadInstance, exception =>
        {
	        exception
		        .IfPresent(ex =>
		        {
			        RollbackResourcePacks(ex, cookie);
		        })
		        .IfEmpty(() =>
		        {
			        _reloadStateTracker.FinishReload();
			        OnResourceLoadFinished(cookie);
		        });
        }, false);
    }

    public void UpdateTitle()
    {
	    Window.SetTitle(CreateTitle());
    }

    private string CreateTitle()
    {
	    var sb = new StringBuilder(MiaCore.ProductName);

	    if (Screen != null)
	    {
		    sb.Append($" -> S: {Screen.GetType()}");
	    }

	    if (Overlay != null)
	    {
		    sb.Append($" -> O: {Overlay.GetType()}");
	    }

	    return sb.ToString();
    }

    private void OnResourceLoadFinished(GameLoadCookie? cookie)
    {
	    if (IsGameLoadFinished) return;
	    IsGameLoadFinished = true;
	    OnGameLoadFinished(cookie);
    }

    private void OnGameLoadFinished(GameLoadCookie? cookie)
    {
	    var action = BuildInitialScreens(cookie);
	    // TODO: GameLoadTimesEvent
	    action();
    }

    private Action BuildInitialScreens(GameLoadCookie? cookie)
    {
	    var list = new List<Func<Action, Screen>>();
	    AddInitialScreens(list);

	    var action = () =>
	    {
		    if (cookie != null && cookie.QuickPlayData.IsEnabled)
		    {
			    // TODO: quick play
		    }
		    else
		    {
			    Screen = new TitleScreen(true);
		    }
	    };

	    list.Reverse();
	    foreach (var func in list)
	    {
		    var screen = func(action);
		    action = () => Screen = screen;
	    }

	    return action;
    }

    private void AddInitialScreens(List<Func<Action, Screen>> list)
    {
	    
    }

    private void RollbackResourcePacks(Exception ex, GameLoadCookie? cookie)
    {
	    if (_resourcePackRepository.SelectedIds.Count > 1)
	    {
		    ClearResourcePacksOnError(ex, null, cookie);
	    }
	    else
	    {
		    throw ex;
	    }
    }

    public void ClearResourcePacksOnError(Exception ex, IComponent? component, GameLoadCookie? cookie)
    {
	    Logger.Info("Caught error loading resourcepacks, removing all selected resourcepacks");
	    _reloadStateTracker.StartRecovery(ex);
    }

    private void WindowOnCursorEntered()
    {
        
    }

    private void WindowOnDisplayResized()
    {
	    var i = Window.CalculateScale(2, false); // guiScale, enforceUnicode
	    Window.SetGuiScale(i);
	    Screen?.Resize(this, Window.GuiScaledWidth, Window.GuiScaledHeight);
	    MainRenderTarget.Resize(Window.Width, Window.Height, OnMacOs);
    }

    private void WindowOnWindowActiveChanged(bool active)
    {
	    
    }

    protected override Thread RunningThread => _gameThread;

    public float DeltaFrameTime => _timer.TickDelta;
    public InputType LastInputType { get; set; }

    protected override IRunnable WrapRunnable(IRunnable runnable) => runnable;

    protected override bool ShouldRun(IRunnable runnable) => true;

    public void Run()
    {
        _gameThread = Thread.CurrentThread;
        if (Environment.ProcessorCount > 4)
        {
            _gameThread.Priority = ThreadPriority.Highest;
        }

        try
        {
	        var bl = false;

	        while (IsRunning)
	        {
		        if (_delayedCrash != null)
		        {
			        Crash(_delayedCrash());
			        return;
		        }

		        try
		        {
			        // TODO: Profiler
			        RunTick(!bl);
		        }
		        catch (OutOfMemoryException ex)
		        {
			        EmergencySave();
			        GC.Collect();
			        Logger.Error("Out of memory");
			        Logger.Error(ex);
			        bl = true;
		        }
	        }
        }
        catch (ReportedException ex)
        {
	        Logger.Error("Reported exception thrown!");
	        Logger.Error(ex);
	        Crash(ex.Report);
        }
        catch (Exception ex)
        {
	        var report = FillReport(new CrashReport("Unexpected error", ex));
			Logger.Error("Unreported exception thrown!");
			Logger.Error(ex);
			Crash(report);
        }
    }

    public CrashReport FillReport(CrashReport report)
    {
	    return report;
    }

    public static void Crash(CrashReport report)
    {
	    Logger.Error(report.GetFriendlyReport());
	    // var dir = Path.Combine(Instance!.GameDirectory, "crash-reports/");
	    // var file = Path.Combine(dir, "crash-client.txt");
	    Environment.Exit(-1);
    }

    public Task ReloadResourcePacksAsync() => ReloadResourcePacksAsync(false, null);
    
    private Task ReloadResourcePacksAsync(bool bl, GameLoadCookie? cookie)
    {
        if (_pendingReload != null) return _pendingReload.Task;

        var source = new TaskCompletionSource();
        if (!bl && Overlay is LoadingOverlay)
        {
	        _pendingReload = source;
	        return source.Task;
        }

        _resourcePackRepository.Reload();
        var list = _resourcePackRepository.OpenAllSelected();
        if (!bl)
        {
	        _reloadStateTracker.StartReload(ResourceLoadStateTracker.ReloadReason.Manual, list);
        }

        Overlay = new LoadingOverlay(this, ResourceManager.CreateReload(Util.BackgroundExecutor, this, _resourceReloadInitialTask, list),
	        x =>
	        {
		        x.IfPresent(ex =>
		        {
			        if (bl)
			        {
				        AbortResourcePackRecovery();
			        }
			        else
			        {
				        RollbackResourcePacks(ex, cookie);
			        }
		        }).IfEmpty(() =>
		        {
			        _reloadStateTracker.FinishReload();
			        source.SetResult();
			        OnResourceLoadFinished(cookie);
		        });
	        }, true);
        return source.Task;
    }

    private void AbortResourcePackRecovery()
    {
	    Overlay = null;
	    Screen = new TitleScreen();
    }

    private void RunTick(bool bl)
    {
	    Task.Yield();
        Window.SetErrorSection("Pre render");
        var l = Util.GetNanos();
        if (Window.ShouldClose) Stop();

        if (_pendingReload != null && Overlay is not LoadingOverlay)
        {
	        var source = _pendingReload;
	        _pendingReload = null;
	        ReloadResourcePacksAsync()
		        .ThenRunAsync(() => source.SetResult());
        }

        while (_progressTasks.TryDequeue(out var action))
        {
	        action();
        }

        if (bl)
        {
	        var i = _timer.AdvanceTime(Util.GetMillis());
	        RunAllTasks();

	        for (var j = 0; j < Math.Min(10, i); j++)
	        {
		        Tick();
	        }
        }
        
        Window.SetErrorSection("Render");
        
        // _profiler.Push("render");
        var m = Util.GetNanos();
        bool bl2;

        RenderSystem.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, OnMacOs);
        MainRenderTarget.BindWrite(true);
        RenderSystem.EnableCull();
        
        if (!NoRender)
        {
	        GameRenderer.Render(_pause ? _pausePartialTick : _timer.PartialTick, l, bl);
        }
        
        MainRenderTarget.UnbindWrite();
        MainRenderTarget.BlitToScreen(Window.Width, Window.Height);
        
        
        // _profiler.PopPush("updateDisplay");
        Window.UpdateDisplay();
        var k = GetFramerateLimit();
        if (k < 260)
        {
	        RenderSystem.LimitDisplayFps(k);
        }

        Thread.Yield();
        
        Window.SetErrorSection("Post render");
        
        
        UpdateTitle();
    }

    private int GetFramerateLimit()
    {
	    return Window.FrameRateLimit;
    }

    public void Tick()
    {
	    
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void EmergencySave()
    {
        try
        {
            MemoryReserve.Release();
            // TODO: Clear level renderer
        }
        catch
        {
            // ...
        }

        try
        {
            GC.Collect();
            // TODO: Halt singleplayer server
            // TODO: Clear level
        }
        catch
        {
            // ...
        }

        GC.Collect();
    }

    public override void Dispose()
    {
	    try
	    {
		    ResourceManager.Dispose();
	    }
	    catch (Exception ex)
	    {
		    Logger.Error("Shutdown failure!");
		    Logger.Error(ex);
		    throw;
	    }
	    finally
	    {
		    _virtualScreen.Dispose();
		    Window.Dispose();
	    }
    }

    public void Destroy()
    {
	    try
	    {
			Logger.Info("Stopping!");
	    }
	    finally
	    {
		    if (_delayedCrash == null) Environment.Exit(0);
	    }
    }

    public record GameLoadCookie(RealmsClient RealmsClient, QuickPlayData QuickPlayData);
}