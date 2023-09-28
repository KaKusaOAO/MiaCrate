using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Resources;
using Veldrid;

namespace MiaCrate.Client.UI;

public class FontTexture : AbstractTexture, IDumpable
{
    private const int Size = 256;

    private readonly GlyphRenderTypes _renderTypes;
    private readonly bool _colored;
    private readonly Node _root;

    public FontTexture(GlyphRenderTypes renderTypes, bool colored)
    {
        _colored = colored;
        _root = new Node(0, 0, Size, Size);

        var preparation = TextureUtil.PrepareImage(colored
            ? PixelFormat.R8_G8_B8_A8_UNorm
            : PixelFormat.R8_UNorm, Size, Size);
        
        _textureDescription = preparation.TextureDescription;
        _samplerDescription = preparation.SamplerDescription;

        var factory = GlStateManager.ResourceFactory;
        Texture?.Dispose();
        Texture = factory.CreateTexture(_textureDescription);
        
        Sampler?.Dispose();
        Sampler = factory.CreateSampler(_samplerDescription);
        
        _renderTypes = renderTypes;
    }

    public override void Load(IResourceManager manager)
    {
        
    }

    public override void Dispose()
    {
        ReleaseId();
    }

    public BakedGlyph? Add(ISheetGlyphInfo info)
    {
        if (info.IsColored != _colored) return null;

        var node = _root.Insert(info);
        if (node == null) return null;

        info.Upload(this, node.X, node.Y);

        const float width = Size;
        const float height = Size;
        const float offset = 0.01f;

        return new BakedGlyph(_renderTypes, 
            (node.X + offset) / width, (node.X - offset + info.PixelWidth) / width, 
            (node.Y + offset) / height, (node.Y - offset + info.PixelHeight) / height, 
            info.Left, info.Right, info.Up, info.Down);
    }

    public void DumpContents(ResourceLocation location, string path)
    {
        var str = location.ToDebugFileName();
        TextureUtil.WriteAsPng(Texture!, path, str, 0, Size, Size, 
            i => (i & 0xFF000000) == 0 ? 0xFF000000 : i);
    }

    private class Node
    {
        public int X { get; }
        public int Y { get; }
        
        private readonly int _width;
        private readonly int _height;
        private Node? _left;
        private Node? _right;
        private bool _occupied;

        public Node(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            _width = width;
            _height = height;
        }

        public Node? Insert(ISheetGlyphInfo info)
        {
            if (_left != null && _right != null)
                return _left.Insert(info) ?? _right.Insert(info);

            if (_occupied) return null;

            var i = info.PixelWidth;
            var j = info.PixelHeight;
            if (i > _width || j > _height) return null;

            if (i == _width && j == _height)
            {
                _occupied = true;
                return this;
            }

            var k = _width - i;
            var l = _height - j;

            if (k > l)
            {
                _left = new Node(X, Y, i, _height);
                _right = new Node(X + i + 1, Y, _width - i - 1, _height);
            }
            else
            {
                _left = new Node(X, Y, _width, j);
                _right = new Node(X, Y + j + 1, _width, _height - j - 1);
            }

            return _left.Insert(info);
        }
    }
}