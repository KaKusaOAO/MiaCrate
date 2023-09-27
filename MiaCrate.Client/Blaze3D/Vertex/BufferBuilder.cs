using System.Runtime.CompilerServices;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Utils;
using Mochi.Utils;
using OpenTK.Mathematics;

namespace MiaCrate.Client;

public class BufferBuilder : DefaultedVertexConsumer, IBufferVertexConsumer
{
    private int _renderedBufferCount;
    private int _renderedBufferPointer;
    private int _nextElementByte;
    private int _vertices;
    private VertexFormatElement? _currentElement;
    private VertexFormat _format = null!;
    private VertexFormat.Mode _mode = null!;
    private byte[] _buffer;
    private Vector3[]? _sortingPoints;
    private bool _indexOnly;
    private bool _fastFormat;
    private bool _fullFormat;
    private int _elementIndex;
    private IVertexSorting? _sorting;
    
    public bool IsBuilding { get; private set; }

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

    public void Begin(VertexFormat.Mode mode, VertexFormat format)
    {
        if (IsBuilding) throw new InvalidOperationException("Already building!");
        IsBuilding = true;
        _mode = mode;
        SwitchFormat(format);

        _currentElement = format.Elements[0];
        _elementIndex = 0;
    }

    public RenderedBuffer End()
    {
        EnsureDrawing();
        var result = StoreRenderedBuffer();
        Reset();
        return result;
    }

    private void Reset()
    {
        IsBuilding = false;
        _vertices = 0;
        _currentElement = null;
        _elementIndex = 0;
        _sortingPoints = null;
        _sorting = null;
        _indexOnly = false;
    }

    private void EnsureDrawing()
    {
        if (!IsBuilding) throw new InvalidOperationException("Not building!");
    }

    public void SetQuadSorting(IVertexSorting sorting)
    {
        if (_mode != VertexFormat.Mode.Quads) return;
        _sorting = sorting;
        _sortingPoints ??= MakeQuadSortingPoints();
    }

    private Vector3[] MakeQuadSortingPoints()
    {
        unsafe
        {
            var buffer = UnsafeUtil.AsFixed<float>(_buffer);
            var i = _renderedBufferPointer / sizeof(float);
            var j = _format.IntegerSize;
            var k = j * _mode.PrimitiveStride;
            var l = _vertices / _mode.PrimitiveStride;

            var arr = new Vector3[l];

            for (var m = 0; m < l; m++)
            {
                var f = buffer[i + m * k + 0];
                var g = buffer[i + m * k + 1];
                var h = buffer[i + m * k + 2];
                var n = buffer[i + m * k + j * 2 + 0];
                var o = buffer[i + m * k + j * 2 + 1];
                var p = buffer[i + m * k + j * 2 + 2];

                var q = (f + n) / 2;
                var r = (g + o) / 2;
                var s = (h + p) / 2;
                arr[m] = new Vector3(q, r, s);
            }

            return arr;
        }
    }

    private RenderedBuffer StoreRenderedBuffer()
    {
        var i = _mode.GetIndexCount(_vertices);
        var j = !_indexOnly ? _vertices * _format.VertexSize : 0;
        var indexType = VertexFormat.IndexType.Least(i);

        bool bl;
        int l, k;
        if (_sortingPoints != null)
        {
            k = Util.RoundToward(i * indexType.Bytes, 4);
            EnsureCapacity(k);
            PutSortedQuadIndices(indexType);
            bl = false;
            _nextElementByte += k;
            l = j + k;
        }
        else
        {
            bl = true;
            l = j;
        }

        k = _renderedBufferPointer;
        _renderedBufferPointer += l;
        _renderedBufferCount++;

        var drawState = new DrawState(_format, _vertices, i, _mode, indexType, _indexOnly, bl);
        return new RenderedBuffer(this, k, drawState);
    }

    private void PutSortedQuadIndices(VertexFormat.IndexType indexType)
    {
        if (_sortingPoints == null || _sorting == null)
            throw new InvalidOperationException("Sorting state uninitialized");

        var arr = _sorting.Sort(_sortingPoints);
        var consumer = IntConsumer(_nextElementByte, indexType);
        
        foreach (var i in arr)
        {
            consumer(i * _mode.PrimitiveStride + 0);
            consumer(i * _mode.PrimitiveStride + 1);
            consumer(i * _mode.PrimitiveStride + 2);
            consumer(i * _mode.PrimitiveStride + 2);
            consumer(i * _mode.PrimitiveStride + 3);
            consumer(i * _mode.PrimitiveStride + 0);
        }
    }

    private Action<int> IntConsumer(int i, VertexFormat.IndexType indexType)
    {
        var ptr = i;
        
        if (indexType == VertexFormat.IndexType.Short)
        {
            return x =>
            {
                var buf = BitConverter.GetBytes((short) x);
                Array.Copy(buf, 0, _buffer, ptr, buf.Length);
                ptr += sizeof(short);
            };
        }

        if (indexType == VertexFormat.IndexType.Int)
        {
            return x =>
            {
                var buf = BitConverter.GetBytes(x);
                Array.Copy(buf, 0, _buffer, ptr, buf.Length);
                ptr += sizeof(int);
            };
        }
        
        throw new Exception($"Unknown index type: {indexType}");
    }

    private void SwitchFormat(VertexFormat format)
    {
        if (_format == format) return;
        _format = format;
        
        var bl = format == DefaultVertexFormat.NewEntity;
        var bl2 = format == DefaultVertexFormat.Block;
        _fastFormat = bl || bl2;
        _fullFormat = bl;
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
        _buffer[_nextElementByte + index] = f;
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

        /// <summary>
        /// The constructor of the rendered buffer. Always remember to provide the builder instance (<c>this</c>).
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pointer"></param>
        /// <param name="drawState"></param>
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