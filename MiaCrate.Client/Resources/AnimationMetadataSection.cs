namespace MiaCrate.Client.Resources;

public class AnimationMetadataSection
{
    public const string SectionName = "animation";
    public const int DefaultFrameTime = 1;
    public const int UnknownSize = -1;

    public static AnimationMetadataSectionSerializer Serializer { get; } = new();

    public static AnimationMetadataSection Empty { get; } =
        new(new List<AnimationFrame>(), UnknownSize, UnknownSize, DefaultFrameTime, false);
    
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

    public FrameSize CalculateFrameSize(int width, int height)
    {
        if (_frameWidth != UnknownSize)
        {
            return _frameHeight != UnknownSize
                ? new FrameSize(_frameWidth, _frameHeight)
                : new FrameSize(_frameWidth, height);
        }

        if (_frameHeight != UnknownSize)
        {
            return new FrameSize(width, _frameHeight);
        }

        var k = Math.Min(width, height);
        return new FrameSize(k, k);
    }
}