namespace MiaCrate.Client.Resources;

public class AnimationMetadataSection
{
    public const string SectionName = "animation";
    public const int DefaultFrameTime = 1;
    public const int UnknownSize = -1;

    public static readonly AnimationMetadataSectionSerializer Serializer = new();
    private readonly List<AnimationFrame> _frames;
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private readonly int _defaultFrameTime;
    private readonly bool _interpolatedFrames;
    
    public AnimationMetadataSection(List<AnimationFrame> frames, int frameWidth, int frameHeight, int defaultFrameTime,
        bool interpolatedFrames)
    {
        _frames = frames;
        _frameWidth = frameWidth;
        _frameHeight = frameHeight;
        _defaultFrameTime = defaultFrameTime;
        _interpolatedFrames = interpolatedFrames;
    }
}

public class AnimationFrame
{
    public const int UnknownFrameTime = -1;
    
    public int Index { get; }
    public int Time { get; }

    public AnimationFrame(int index, int time = UnknownFrameTime)
    {
        Index = index;
        Time = time;
    }
}