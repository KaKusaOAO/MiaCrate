using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using MiaCrate.Auth;
using MiaCrate.Auth.Yggdrasil;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Client.Utils;
using MiaCrate.Data;
using MiaCrate.Resources;
using MiaCrate.World.Entities;
using Mochi.Texts;
using Mochi.Utils;
using OpenTK.Windowing.Desktop;
using Component = MiaCrate.Texts.Component;

namespace MiaCrate.Client;

public class Game : ReentrantBlockableEventLoop<IRunnable>
{
    public static Game? Instance { get; private set; }
    public static readonly bool OnMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private const int MaxTicksPerUpdate = 10; // Redstone ticks
    public static readonly ResourceLocation DefaultFont = new("default");
    public static readonly ResourceLocation UniformFont = new("uniform");
    public static readonly ResourceLocation AltFont = new("alt");
    private static readonly ResourceLocation _regionalCompliances = new("regional_compliances.json");
    private static readonly Task _resourceReloadInitialTask = Task.CompletedTask;
    private static readonly IComponent _socialInteractionsNotAvailable = TranslateText.Of("multiplayer.socialInteractions.not_available");
    public const string UpdateDriversAdvice = "Please make sure you have up-to-date drivers (see aka.ms/mcdriver for instructions).";
    private readonly string _resourcePackDirectory;
    private readonly PropertyMap _profileProperties;
    // private readonly TextureManager _textureManager;
    private readonly IDataFixer _fixerUpper;
    private readonly VirtualScreen _virtualScreen;
    private readonly Window _window;
    private readonly Timer _timer = new(20f, 0L);
    // private readonly RenderBuffers _renderBuffers;
	// public readonly LevelRenderer levelRenderer;
	// private readonly EntityRenderDispatcher _entityRenderDispatcher;
	// private readonly ItemRenderer _itemRenderer;
	// public readonly ParticleEngine particleEngine;
	// private readonly SearchRegistry _searchRegistry = new SearchRegistry();
	private readonly User _user;
	public readonly Font font;
	public readonly Font fontFilterFishy;
	// public readonly GameRenderer gameRenderer;
	// public readonly DebugRenderer debugRenderer;
	// private readonly AtomicReference<StoringChunkProgressListener> _progressListener = new AtomicReference();
	// public readonly Gui gui;
	public Options Options { get; }
	// private readonly HotbarManager _hotbarManager;
	// public readonly MouseHandler mouseHandler;
	// public readonly KeyboardHandler keyboardHandler;
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
	private readonly ReloadableResourceManager _resourceManager;
	private readonly VanillaPackResources _vanillaPackResources;
	// private readonly DownloadedPackSource _downloadedPackSource;
	private readonly PackRepository _resourcePackRepository;
	// private readonly LanguageManager _languageManager;
	// private readonly BlockColors _blockColors;
	// private readonly ItemColors _itemColors;
	// private readonly RenderTarget _mainRenderTarget;
	// private readonly SoundManager _soundManager;
	// private readonly MusicManager _musicManager;
	// private readonly FontManager _fontManager;
	// private readonly SplashManager _splashManager;
	// private readonly GpuWarnlistManager _gpuWarnlistManager;
	// private readonly PeriodicNotificationManager _regionalCompliancies;
	private readonly YggdrasilAuthenticationService _authenticationService;
	private readonly IMinecraftSessionService _minecraftSessionService;
	// private readonly UserApiService _userApiService;
	// private readonly SkinManager _skinManager;
	// private readonly ModelManager _modelManager;
	// private readonly BlockRenderDispatcher _blockRenderer;
	// private readonly PaintingTextureManager _paintingTextures;
	// private readonly MobEffectTextureManager _mobEffectTextures;
	// private readonly ToastComponent _toast;
	// private readonly Tutorial _tutorial;
	// private readonly PlayerSocialManager _playerSocialManager;
	// private readonly EntityModelSet _entityModels;
	// private readonly BlockEntityRenderDispatcher _blockEntityRenderDispatcher;
	// private readonly ClientTelemetryManager _telemetryManager;
	// private readonly ProfileKeyPairManager _profileKeyPairManager;
	// private readonly RealmsDataFetcher _realmsDataFetcher;
	// private readonly QuickPlayLog _quickPlayLog;
	// public MultiPlayerGameMode gameMode;
	// public ClientLevel level;
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
	public bool noRender;
	public Screen? Screen { get; set; }
	public Overlay? Overlay { get; set; }
	private bool _connectedToRealms;
	private Thread _gameThread;
	private bool _running;
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
    public bool IsWindowActive { get; set; }

    public bool IsRenderedOnThread => false;
    public bool IsRunning { get; private set; }

    public Game(GameConfig config) : base("Client")
    {
        Instance = this;
        GameDirectory = config.Location.GameDirectory;
        _resourcePackDirectory = config.Location.ResourcePackDirectory;
        _launchedVersion = config.Game.LaunchVersion;
        _versionType = config.Game.VersionType;
        _profileProperties = config.User.ProfileProperties;
        
        // TODO: ClientPackSource
        var clientPackSource = new ClientPackSource(config.Location.ExternalAssetSource);
        _resourcePackRepository = new PackRepository(clientPackSource);
        _proxy = config.User.Proxy;
        _authenticationService = new YggdrasilAuthenticationService(_proxy);
        // _minecraftSessionService = _authenticationService.CreateMinecraftSessionService();
        _user = config.User.User;
        Logger.Info($"Setting user: {_user.Name}");
        Logger.Verbose($"(Session ID is {_user.SessionId})");

        _demo = config.Game.IsDemo;
        _allowsMultiplayer = !config.Game.IsMultiplayerDisabled;
        _allowsChat = !config.Game.IsChatDisabled;
        _gameThread = Thread.CurrentThread;
        Options = new Options(this, GameDirectory);
        _running = true;
        Logger.Info($"Backend library: {RenderSystem.GetBackendDescription()}");

        var displayData = config.Display;
        Util.TimeSource = RenderSystem.InitBackendSystem();
        _virtualScreen = new VirtualScreen(this);
        _window = _virtualScreen.NewWindow(displayData, null, "11");
        _window.WindowActiveChanged += WindowOnWindowActiveChanged;
        _window.DisplayResized += WindowOnDisplayResized;
        _window.CursorEntered += WindowOnCursorEntered;
        IsWindowActive = true;

        RenderSystem.InitRenderer(0, false);
        _resourceManager = new ReloadableResourceManager(PackType.ClientResources);
        _window.SetErrorSection("Startup");

        var list = _resourcePackRepository.OpenAllSelected();
        _reloadStateTracker.StartReload(ResourceLoadStateTracker.ReloadReason.Initial, list);
        var reloadInstance = _resourceManager
	        .CreateReload(Util.BackgroundExecutor, this, _resourceReloadInitialTask, list);
        Overlay = new LoadingOverlay(this, reloadInstance, x =>
        {
	        x.IfEmpty(() =>
	        {
		        _reloadStateTracker.FinishReload();
	        });
        }, false);
    }

    private void WindowOnCursorEntered()
    {
        throw new NotImplementedException();
    }

    private void WindowOnDisplayResized()
    {
        throw new NotImplementedException();
    }

    private void WindowOnWindowActiveChanged(bool active)
    {
        throw new NotImplementedException();
    }

    protected override Thread RunningThread => throw new NotImplementedException();

    protected override IRunnable WrapRunnable(IRunnable runnable)
    {
        throw new NotImplementedException();
    }

    protected override bool ShouldRun(IRunnable runnable)
    {
        throw new NotImplementedException();
    }

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

    public Task ReloadResourcePacksAsync() => ReloadResourcePacksAsync(false);
    
    private Task ReloadResourcePacksAsync(bool bl)
    {
        if (_pendingReload != null) return _pendingReload.Task;

        var source = new TaskCompletionSource();
        if (!bl && Overlay is LoadingOverlay)
        {
	        _pendingReload = source;
	        return _pendingReload.Task;
        }

        _resourcePackRepository.Reload();
        var list = _resourcePackRepository.OpenAllSelected();
        if (!bl)
        {
	        _reloadStateTracker.StartReload(ResourceLoadStateTracker.ReloadReason.Manual, list);
        }

        Overlay = new LoadingOverlay(this, _resourceManager.CreateReload(Util.BackgroundExecutor, this, _resourceReloadInitialTask, list),
	        x =>
	        {
		        x.IfPresent(x =>
		        {
			        if (bl)
			        {
				        AbortResourcePackRecovery();
			        }
			        else
			        {
				        // TODO: Rollback
			        }
		        }).IfEmpty(() =>
		        {
					source.SetResult();
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
        _window.SetErrorSection("Pre render");
        var l = Util.GetNanos();
        if (_window.ShouldClose) Stop();

        if (_pendingReload != null && Overlay is not LoadingOverlay)
        {
	        var source = _pendingReload;
	        _pendingReload = null;
	        ReloadResourcePacksAsync().ContinueWith(_ =>
	        {
		        source.SetResult();
	        });
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
        
        _window.SetErrorSection("Render");
        var m = Util.GetNanos();
        bool bl2;
        
        _window.SetErrorSection("Post render");
        
        
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
		    _resourceManager.Dispose();
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
		    _window.Dispose();
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
}