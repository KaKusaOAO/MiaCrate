using MiaCrate.Data;
using MiaCrate.DataFixes;

namespace MiaCrate;

public static class SharedConstants
{
	public const bool UseHighDpi = true;
	
    public const bool IsSnapshot = false;
    public const int WorldVersion = 3465;
    public const string Series = "main";
    public const string VersionString = "1.20.1";
    public const int ReleaseNetworkProtocolVersion = 763;
    public const int SnapshotNetworkProtocolVersion = 142;
    public const int SNbtNagVersion = 3437;
    public const int SnapshotProtocolBit = 30;
    public const int ResourcePackFormat = 15;
    public const int DataPackFormat = 15;
    public const int LanguageFormat = 1;
    public const string DataVersionTag = "DataVersion";
    public const bool UseNewRenderSystem = false;
	public const bool MultiThreadedRendering = false;
	
	// Those fixes might be optimized out... :P
	public const bool FixTntDupe = false;
	public const bool FixSandDupe = false;
	
	public const bool UseDebugFeatures = false;
	public const bool DebugOpenIncompatibleWorlds = UseDebugFeatures && false;
	public const bool DebugAllowLowSimDistance = UseDebugFeatures && false;
	public const bool DebugHotkeys = UseDebugFeatures && false;
	public const bool DebugUiNarration = UseDebugFeatures && false;
	public const bool DebugRender = UseDebugFeatures && false;
	public const bool DebugPathfinding = UseDebugFeatures && false;
	public const bool DebugWater = UseDebugFeatures && false;
	public const bool DebugHeightmap = UseDebugFeatures && false;
	public const bool DebugCollision = UseDebugFeatures && false;
	public const bool DebugSupportBlocks = UseDebugFeatures && false;
	public const bool DebugShapes = UseDebugFeatures && false;
	public const bool DebugNeighborsUpdate = UseDebugFeatures && false;
	public const bool DebugStructures = UseDebugFeatures && false;
	public const bool DebugLight = UseDebugFeatures && false;
	public const bool DebugSkyLightSections = UseDebugFeatures && false;
	public const bool DebugWorldGenAttempt = UseDebugFeatures && false;
	public const bool DebugSolidFace = UseDebugFeatures && false;
	public const bool DebugChunks = UseDebugFeatures && false;
	public const bool DebugGameEventListeners = UseDebugFeatures && false;
	public const bool DebugDumpTextureAtlas = UseDebugFeatures && false;
	public const bool DebugDumpInterpolatedTextureFrames = UseDebugFeatures && false;
	public const bool DebugStructureEditMode = UseDebugFeatures && false;
	public const bool DebugSaveStructuresAsSNbt = UseDebugFeatures && false;
	public const bool DebugSynchronousGlLogs = UseDebugFeatures && false;
	public const bool DebugVerboseServerEvents = UseDebugFeatures && false;
	public const bool DebugNamedRunnables = UseDebugFeatures && false;
	public const bool DebugGoalSelector = UseDebugFeatures && false;
	public const bool DebugVillageSections = UseDebugFeatures && false;
	public const bool DebugBrain = UseDebugFeatures && false;
	public const bool DebugBees = UseDebugFeatures && false;
	public const bool DebugRaids = UseDebugFeatures && false;
	public const bool DebugBlockBreak = UseDebugFeatures && false;
	public const bool DebugResourceLoadTimes = UseDebugFeatures && false;
	public const bool DebugMonitorTickTimes = UseDebugFeatures && false;
	public const bool DebugKeepJigsawBlocksDuringStructureGen = UseDebugFeatures && false;
	public const bool DebugDontSaveWorld = UseDebugFeatures && false;
	public const bool DebugLargeDripstone = UseDebugFeatures && false;
	public const bool DebugPacketSerialization = UseDebugFeatures && false;
	public const bool DebugCarvers = UseDebugFeatures && false;
	public const bool DebugOreVeins = UseDebugFeatures && false;
	public const bool DebugSculkCatalyst = UseDebugFeatures && false;
	public const bool DebugBypassRealmsVersionCheck = UseDebugFeatures && false;
	public const bool DebugSocialInteractions = UseDebugFeatures && false;
	public const bool DebugValidateResourcePathCase = UseDebugFeatures && false;
	public const bool DebugIgnoreLocalMobCap = UseDebugFeatures && false;
	public const bool DebugSmallSpawn = UseDebugFeatures && false;
	public const bool DebugDisableLiquidSpreading = UseDebugFeatures && false;
	public const bool DebugAquifers = UseDebugFeatures && false;
	// public const bool DebugJfrProfilingEnableLevelLoading = UseDebugFeatures && false;
	public static bool DebugGenerateSquareTerrainWithoutNoise { get; set; } = UseDebugFeatures && false;
	public static bool DebugGenerateStripedTerrainWithoutNoise { get; set; } = UseDebugFeatures && false;
	public const bool DebugOnlyGenerateHalfTheWorld = UseDebugFeatures && false;
	public const bool DebugDisableFluidGeneration = UseDebugFeatures && false;
	public const bool DebugDisableAquifers = UseDebugFeatures && false;
	public const bool DebugDisableSurface = UseDebugFeatures && false;
	public const bool DebugDisableCarvers = UseDebugFeatures && false;
	public const bool DebugDisableStructures = UseDebugFeatures && false;
	public const bool DebugDisableFeatures = UseDebugFeatures && false;
	public const bool DebugDisableOreVeins = UseDebugFeatures && false;
	public const bool DebugDisableBlending = UseDebugFeatures && false;
	public const bool DebugDisableBelowZeroRetroGeneration = UseDebugFeatures && false;
	public const int DefaultMinecraftPort = 25565;
	public const bool InGameDebugOutput = UseDebugFeatures && false;
	public const bool DebugSubtitles = UseDebugFeatures && false;
	public const int FakeMsLatency = 0;
	public const int FakeMsJitter = 0;
	// public const ResourceLeakDetector.Level NettyLeakDetection;
	public const bool CommandStackTraces = false;
	public const bool DebugWorldRecreate = UseDebugFeatures && false;
	public const bool DebugShowServerDebugValues = UseDebugFeatures && false;
	public const bool DebugStoreChunkStackTraces = UseDebugFeatures && false;
	public const bool DebugFeatureCount = UseDebugFeatures && false;
	public const bool DebugResourceGenerationOverride = UseDebugFeatures && false;
	public const bool DebugForceTelemetry = UseDebugFeatures && false;
	public const bool DebugDontSendTelemetryToBackend = UseDebugFeatures && false;
	public const long MaximumTickTimeNanos = 300 * 1000000L;
	public const bool UseWorkflowsHooks = false;
	public static bool CheckDataFixerSchema { get; set; }
	public static bool IsRunningInIde { get; set; }

	public const int ProtocolVersion = IsSnapshot
		? (1 << SnapshotProtocolBit) | SnapshotNetworkProtocolVersion
		: ReleaseNetworkProtocolVersion;

	public static HashSet<Dsl.ITypeReference> DataFixTypesToOptimize { get; private set; } = new();
	public const int WorldResolution = 16;
	public const int MaxChatLength = 256;
	public const int MaxCommandLength = 32500;
	public const int MaxChainedNeighborUpdates = 1000000;
	public const int MaxRenderDistance = 32;
	public static readonly char[] IllegalFileCharacters = {
		'/', '\n', '\r', '\t', '\u0000', '\f', '`', '?', '*', '\\', '<', '>', '|', '"', ':'
	};
	
	public const int TicksPerSecond = 20;
	public const int TicksPerMinute = TicksPerSecond * 60;
	public const int TicksPerGameDay = TicksPerMinute * 20;
	public const float AverageGameTicksPerRandomTickPerBlock = 1365.3334f;
	public const float AverageRandomTicksPerBlockPerMinute = 0.87890625f;
	public const float AverageRandomTicksPerBlockPerGameDay = 17.578125f;
	public const int WorldIconSize = 64;

	private static IWorldVersion? _currentVersion;

	public static void TryDetectVersion() => 
		_currentVersion ??= DetectedVersion.TryDetectVersion();

	public static IWorldVersion CurrentVersion
	{
		get => _currentVersion ?? throw new Exception("Game version not set");
		set
		{
			if (_currentVersion == null)
				_currentVersion = value;
			else if (_currentVersion != value)
				throw new Exception("Cannot override the current game version!");
		}
	}

	public static void EnableDataFixerOptimization()
	{
		DataFixTypesToOptimize = DataFixTypes.TypesForLevelList;
	}
}