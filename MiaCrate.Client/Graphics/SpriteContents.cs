using MiaCrate.Client.Resources;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class SpriteContents : IStitcherEntry, IDisposable
{
    private readonly NativeImage _image;
    private readonly IResourceMetadata _metadata;
    private readonly AnimatedTexture? _animatedTexture;
    
    public int Width { get; }

    public int Height { get; }

    public ResourceLocation Name { get; }

    public SpriteContents(ResourceLocation name, FrameSize frameSize, NativeImage image, IResourceMetadata metadata)
    {
        _image = image;
        _metadata = metadata;
        Name = name;
        Width = frameSize.Width;
        Height = frameSize.Height;
    }

    public void IncreaseMipLevel(int i)
    {
        
    }

    public ISpriteTicker? CreateTicker() => _animatedTexture?.CreateTicker();

    public void Dispose()
    {
        
    }

    private class AnimatedTexture
    {
        private readonly SpriteContents _instance;
        private readonly List<FrameSize> _frames;
        private readonly int _frameRowSize;
        private readonly bool _interpolateFrames;

        public AnimatedTexture(SpriteContents instance, List<FrameSize> frames, int frameRowSize, bool interpolateFrames)
        {
            _instance = instance;
            _frames = frames;
            _frameRowSize = frameRowSize;
            _interpolateFrames = interpolateFrames;
        }

        public ISpriteTicker CreateTicker() => 
            new Ticker(_instance, this, _interpolateFrames ? new InterpolationData() : null);
    }

    private class InterpolationData : IDisposable
    {
        private readonly SpriteContents _instance;

        public void Dispose()
        {
            
        }
    }
    
    private class Ticker : ISpriteTicker
    {
        private readonly SpriteContents _instance;
        private readonly AnimatedTexture _animationInfo;
        private readonly InterpolationData? _interpolationData;

        public Ticker(SpriteContents instance, AnimatedTexture animationInfo, InterpolationData? interpolationData)
        {
            _instance = instance;
            _animationInfo = animationInfo;
            _interpolationData = interpolationData;
        }
        
        public void TickAndUpload(int i, int j)
        {
            
        }
        
        public void Dispose()
        {
            _interpolationData?.Dispose();
        }
    }
}