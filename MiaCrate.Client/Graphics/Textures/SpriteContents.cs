using MiaCrate.Client.Resources;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class SpriteContents : IStitcherEntry, IDisposable
{
    private readonly NativeImage _image;
    private readonly AnimatedTexture? _animatedTexture;
    private readonly NativeImage _originalImage;
    private NativeImage[] _byMipLevel;
    
    public int Width { get; }
    public int Height { get; }
    public ResourceLocation Name { get; }
    public IResourceMetadata Metadata { get; }

    public SpriteContents(ResourceLocation name, FrameSize frameSize, NativeImage image, IResourceMetadata metadata)
    {
        _image = image;
        Metadata = metadata;
        Name = name;
        Width = frameSize.Width;
        Height = frameSize.Height;

        _originalImage = image;
        _byMipLevel = new[] {image};
    }

    public void IncreaseMipLevel(int i)
    {
        
    }

    public ISpriteTicker? CreateTicker() => _animatedTexture?.CreateTicker();

    public void Dispose()
    {
        
    }

    public void UploadFirstFrame(int x, int y)
    {
        if (_animatedTexture != null)
        {
            _animatedTexture.UploadFirstFrame(x, y);
        }
        else
        {
            Upload(x, y, 0, 0, _byMipLevel);
        }
    }

    private void Upload(int x, int y, int k, int l, NativeImage[] byMipLevel)
    {
        // This for loop is weird
        for (var m = 0; m < _byMipLevel.Length; m++)
        {
            byMipLevel[m].Upload(m, x >> m, y >> m, k >> m, l >> m, 
                Width >> m, Height >> m, _byMipLevel.Length > 1, false);
        }
    }

    private class AnimatedTexture
    {
        private readonly SpriteContents _instance;
        private readonly List<FrameInfo> _frames;
        private readonly int _frameRowSize;
        private readonly bool _interpolateFrames;

        public AnimatedTexture(SpriteContents instance, List<FrameInfo> frames, int frameRowSize, bool interpolateFrames)
        {
            _instance = instance;
            _frames = frames;
            _frameRowSize = frameRowSize;
            _interpolateFrames = interpolateFrames;
        }

        public ISpriteTicker CreateTicker() => 
            new Ticker(_instance, this, _interpolateFrames ? new InterpolationData() : null);

        public void UploadFirstFrame(int x, int y)
        {
            UploadFrame(x, y, _frames[0].Index);
        }

        public int GetFrameX(int i) => i % _frameRowSize;
        public int GetFrameY(int i) => i / _frameRowSize;

        public void UploadFrame(int x, int y, int index)
        {
            var l = GetFrameX(index) * _instance.Width;
            var m = GetFrameY(index) * _instance.Height;
            _instance.Upload(x, y, l, m, _instance._byMipLevel);
        }
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

    private class FrameInfo
    {
        public int Index { get; }
        public int Time { get; }
        
        public FrameInfo(int index, int time)
        {
            Index = index;
            Time = time;
        }
    }
}