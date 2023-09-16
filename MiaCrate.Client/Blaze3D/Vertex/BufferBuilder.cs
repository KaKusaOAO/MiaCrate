using MiaCrate.Client.Graphics;
using Mochi.Utils;

namespace MiaCrate.Client;

public class BufferBuilder : DefaultedVertexConsumer, IBufferVertexConsumer
{
    private int _renderedBufferCount;
    private int _renderedBufferPointer;
    private int _nextElementByte;
    private int _vertices;
    private VertexFormatElement? _currentElement;
    private VertexFormat _format;
    private VertexFormat.Mode _mode;
    private byte[] _buffer;
    private bool _fastFormat;
    private bool _fullFormat;
    private int _elementIndex;

    public BufferBuilder(int capacity)
    {
        _buffer = new byte[capacity * 6];
    }

    private void EnsureVertexCapacity() => EnsureCapacity(_format.VertexSize);

    private void EnsureCapacity(int capacity)
    {
        if (_nextElementByte + capacity <= _buffer.Length) return;

        var j = _buffer.Length;
        var k = j + RoundUp(capacity);
        Logger.Verbose($"Needed to grow BufferBuilder buffer: Old size {j} bytes, new size {k} bytes.");
        Array.Resize(ref _buffer, k);
    }

    private ReadOnlySpan<byte> BufferSlice(int start, int end) => new(_buffer, start, end - start);

    private void ReleaseRenderedBuffer()
    {
        if (_renderedBufferCount > 0 && --_renderedBufferCount == 0) Clear();
    }

    public void Vertex(float x, float y, float z, float red, float green, float blue, float alpha, float u, float v, int oPacked,
        int uv2Packed, float nx, float ny, float nz)
    {
        if (IsDefaultColorSet)
            throw new InvalidOperationException();

        if (_fastFormat)
        {
            PutFloat(0, x);
            PutFloat(4, y);
            PutFloat(8, z);
            PutByte(12, (byte)(int)(red * 255));
            PutByte(13, (byte)(int)(green * 255));
            PutByte(14, (byte)(int)(blue * 255));
            PutByte(15, (byte)(int)(alpha * 255));
            PutFloat(16, u);
            PutFloat(20, v);

            int offset;
            if (_fullFormat)
            {
                PutShort(24, (short) (oPacked & 0xffff));
                PutShort(28, (short) ((oPacked >> 16) & 0xffff));
                offset = 28;
            }
            else
            {
                offset = 24;
            }
            
            PutShort(offset + 0, (short) (uv2Packed & 0xffff));
            PutShort(offset + 2, (short) ((uv2Packed >> 16) & 0xffff));
            PutByte(offset + 4, IBufferVertexConsumer.NormalIntValue(nx));
            PutByte(offset + 5, IBufferVertexConsumer.NormalIntValue(ny));
            PutByte(offset + 6, IBufferVertexConsumer.NormalIntValue(nz));
            _nextElementByte += offset + 8;
            EndVertex();
        }
        else
        {
            IBufferVertexConsumer
                .DefaultVertex(this, x, y, z, red, green, blue, alpha, u, v, oPacked, uv2Packed, nx, ny, nz);
        }
    }

    public void NextElement()
    {
        var elements = _format.Elements;
        _elementIndex = (_elementIndex + 1) % elements.Count;
        _nextElementByte += _currentElement!.ByteSize;

        var element = elements[_elementIndex];
        _currentElement = element;

        if (element.Usage == VertexFormatElement.UsageInfo.Padding) 
            NextElement();

        if (IsDefaultColorSet && _currentElement.Usage == VertexFormatElement.UsageInfo.Color)
        {
            IBufferVertexConsumer
                .DefaultColorImpl(this, DefaultR, DefaultG, DefaultB, DefaultA);
        }
    }

    public override IVertexConsumer Vertex(double x, double y, double z) =>
        IBufferVertexConsumer.DefaultVertex(this, x, y, z);

    public override IVertexConsumer Color(int red, int green, int blue, int alpha)
    {
        if (IsDefaultColorSet) throw new InvalidOperationException();
        return IBufferVertexConsumer
                .DefaultColorImpl(this, red, green, blue, alpha);
    }

    public override IVertexConsumer Uv(float u, float v) =>
        IBufferVertexConsumer.DefaultUv(this, u, v);

    public override IVertexConsumer OverlayCoords(int i, int j) =>
        IBufferVertexConsumer.DefaultOverlayCoords(this, i, j);

    public override IVertexConsumer Uv2(int i, int j) =>
        IBufferVertexConsumer.DefaultUv2(this, i, j);

    public override IVertexConsumer Normal(float x, float y, float z) =>
        IBufferVertexConsumer.DefaultNormal(this, x, y, z);

    public VertexFormatElement CurrentElement =>
        _currentElement ?? throw new InvalidOperationException("BufferBuilder not started");

    public override void EndVertex()
    {
        if (_elementIndex != 0)
            throw new InvalidOperationException("Not filled all elements of the vertex");

        ++_vertices;
        EnsureVertexCapacity();
        if (_mode != VertexFormat.Mode.Lines && _mode != VertexFormat.Mode.LineStrip) return;

        var size = _format.VertexSize;
        Array.Copy(_buffer, _nextElementByte - size, _buffer, _nextElementByte, size);
        _nextElementByte += size;
        ++_vertices;
        EnsureVertexCapacity();
    }

    public void PutFloat(int index, float f)
    {
        var arr = BitConverter.GetBytes(f);
        Array.Copy(arr, 0, _buffer, _nextElementByte + index, arr.Length);
    }
    
    public void PutShort(int index, short s)
    {
        var arr = BitConverter.GetBytes(s);
        Array.Copy(arr, 0, _buffer, _nextElementByte + index, arr.Length);
    }
    
    public void PutByte(int index, byte f)
    {
        _buffer[index] = f;
    }

    public void Clear()
    {
        if (_renderedBufferCount > 0)
            Logger.Warn("Clearing BufferBuilder with unused batches");

        Discard();
    }

    public void Discard()
    {
        _renderedBufferCount = 0;
        _renderedBufferPointer = 0;
        _nextElementByte = 0;
    }
    
    private static int RoundUp(int i)
    {
        var j = 0x200000;
        if (i == 0) return j;
        if (i < 0) j *= -1;

        var k = i % j;
        return k == 0 ? i : i + j - k;
    }

    public record DrawState(VertexFormat Format, int VertexCount, int IndexCount, VertexFormat.Mode Mode,
        VertexFormat.IndexType IndexType, bool IndexOnly, bool SequentialIndex)
    {
        public int VertexBufferSize => VertexCount * Format.VertexSize;
        public int VertexBufferStart => 0;
        public int VertexBufferEnd => VertexBufferSize;
        public int IndexBufferSize => SequentialIndex ? 0 : IndexCount * IndexType.Bytes;
        public int IndexBufferStart => IndexOnly ? 0 : VertexBufferEnd;
        public int IndexBufferEnd => IndexBufferStart + IndexBufferSize;
        
    }

    public class RenderedBuffer : IDisposable
    {
        private readonly BufferBuilder _builder;
        private readonly int _pointer;
        private bool _released;
        
        public DrawState DrawState { get; }

        public ReadOnlySpan<byte> VertexBuffer
        {
            get
            {
                var start = _pointer + DrawState.VertexBufferStart;
                var end = _pointer + DrawState.VertexBufferEnd;
                return _builder.BufferSlice(start, end);
            }
        }
        
        public ReadOnlySpan<byte> IndexBuffer
        {
            get
            {
                var start = _pointer + DrawState.IndexBufferStart;
                var end = _pointer + DrawState.IndexBufferEnd;
                return _builder.BufferSlice(start, end);
            }
        }

        public bool IsEmpty => DrawState.VertexCount == 0;

        public RenderedBuffer(BufferBuilder builder, int pointer, DrawState drawState)
        {
            _builder = builder;
            _pointer = pointer;
            DrawState = drawState;
        }

        public void Dispose()
        {
            if (_released)
                throw new InvalidOperationException("Buffer has already been released");
            
            _builder.ReleaseRenderedBuffer();
            _released = true;
        }
    }
}