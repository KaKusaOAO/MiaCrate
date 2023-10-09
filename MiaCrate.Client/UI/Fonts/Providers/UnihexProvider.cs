using System.IO.Compression;
using System.Runtime.InteropServices;
using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class UnihexProvider : IGlyphProvider
{
    private const int GlyphHeight = 16;
    private const int DigitsPerByte = 2;
    private const int DigitsForWidth8  =  8 << DigitsPerByte;
    private const int DigitsForWidth16 = 16 << DigitsPerByte;
    private const int DigitsForWidth24 = 24 << DigitsPerByte;
    private const int DigitsForWidth32 = 32 << DigitsPerByte;

    private readonly CodepointMap<Glyph> _glyphs;

    private UnihexProvider(CodepointMap<Glyph> glyphs)
    {
        _glyphs = glyphs;
    }

    public IGlyphInfo? GetGlyph(int id) => _glyphs.Get(id);

    public ISet<int> GetSupportedGlyphs() => _glyphs.Keys.ToHashSet();

    private static unsafe void UnpackBitsToBytes(ref int* buffer, int i, int j, int k)
    {
        var l = DigitsForWidth8 - j - 1;
        var m = DigitsForWidth8 - k - 1;

        for (var n = l; n >= m; n--)
        {
            if (n < DigitsForWidth8 && n >= 0)
            {
                var bl = (i >> n & 1) != 0;
                *buffer++ = bl ? -1 : 0;
            }
            else
            {
                *buffer++ = 0;
            }
        }
    }

    private static unsafe void UnpackBitsToBytes(ref int* buffer, ILineData data, int i, int j)
    {
        for (var k = 0; k < GlyphHeight; k++)
        {
            var l = data.Line(k);
            UnpackBitsToBytes(ref buffer, l, i ,j);
        }
    }

    private static void ReadFromStream(Stream stream, ReaderOutput output)
    {
        var i = 0;
        var list = new List<byte>(DigitsForWidth32);

        while (true)
        {
            var bl = CopyUntil(stream, list, ':');
            var j = list.Count;
            if (j == 0 && !bl) return;

            if (!bl || j != 4 && j != 5 && j != 6)
                throw new ArgumentException(
                    $"Invalid entry at line {i}: expected 4, 5 or 6 hex digits followed by a colon");

            var k = 0;
            for (var l = 0; l < j; l++)
            {
                k = k << 4 | DecodeHex(i, list[l]);
            }
            
            list.Clear();
            CopyUntil(stream, list, 10);
            
            var ll = list.Count;
            var lineData = ll switch
            {
                DigitsForWidth8 => ByteContents.Read(i, list),
                DigitsForWidth16 => ShortContents.Read(i, list),
                DigitsForWidth24 => IntContents.Read24(i, list),
                DigitsForWidth32 => IntContents.Read32(i, list),
                _ => throw new ArgumentException($"Invalid entry at line {i}: expected hex number describing " +
                                                 $"(8,16,24,32) x {GlyphHeight} bitmap, followed by a new line")
            };

            output(k, lineData);
            i++;
            list.Clear();
        }
    }

    private static int DecodeHex(int i, List<byte> list, int j) => DecodeHex(i, list[j]);

    private static int DecodeHex(int i, byte b)
    {
        if (b >= '0' && b <= '9') return b - '0';
        if (b >= 'A' && b <= 'F') return b - 'A' + 0xa;
        if (b >= 'a' && b <= 'f') return b - 'a' + 0xa;
        
        throw new ArgumentException($"Invalid entry at line {i}: expected hex digit, got {(char) b}");
    }

    private static bool CopyUntil(Stream stream, List<byte> list, int i)
    {
        while (true)
        {
            var j = stream.ReadByte();
            if (j == -1) return false;
            if (j == i) return true;
            list.Add((byte) j);
        }
    }
    

    public delegate void ReaderOutput(int i, ILineData data);
    
    public interface ILineData
    {
        public int BitWidth { get; }

        public int Mask
        {
            get
            {
                var i = 0;
                
                for (var j = 0; j < 16; j++)
                {
                    i |= Line(j);
                }

                return i;
            }
        }
        
        public int Line(int i);

        public int CalculateWidth()
        {
            var i = Mask;
            var j = BitWidth;
            int k, l;

            if (i == 0)
            {
                k = 0;
                l = j;
            }
            else
            {
                k = Util.NumberOfLeadingZeros(i);
                l = 32 - Util.NumberOfTrailingZeros(i) - 1;
            }

            return Dimensions.Pack(k, l);
        }
    }

    private record Glyph(ILineData Contents, int Left, int Right) : IGlyphInfo
    {
        public int Width => Right - Left + 1;
        
        public float Advance => Width / 2f + 1;

        public float ShadowOffset => 0.5f;

        public float BoldOffset => 0.5f;

        public BakedGlyph Bake(Func<ISheetGlyphInfo, BakedGlyph> func) => func(new Info(this));

        private class Info : ISheetGlyphInfo
        {
            private readonly Glyph _instance;

            public Info(Glyph instance)
            {
                _instance = instance;
            }

            public int PixelWidth => _instance.Width;

            public int PixelHeight => GlyphHeight;

            public bool IsColored => true;

            public float Oversample => 2;

            public unsafe void Upload(int x, int y)
            {
                var size = _instance.Width * GlyphHeight * sizeof(int);
                var ptr = Marshal.AllocHGlobal(size);
                
                var buffer = (int*) ptr;
                UnpackBitsToBytes(ref buffer, _instance.Contents, _instance.Left, _instance.Right);
                
                GlStateManager.Upload(0, x, y, _instance.Width, GlyphHeight, NativeImage.FormatInfo.Rgba, ptr, Marshal.FreeHGlobal);
            }
        }
    }

    private record ByteContents(byte[] Contents) : ILineData
    {
        public int BitWidth => sizeof(byte) * 8;

        public int Line(int i) => Contents[i] << 24;

        public static ILineData Read(int i, List<byte> list)
        {
            var arr = new byte[GlyphHeight];
            var j = 0;

            for (var k = 0; k < GlyphHeight; k++)
            {
                var l = DecodeHex(i, list, j++);
                var m = DecodeHex(i, list, j++);
                var b = (byte) (l << 4 | m);
                arr[k] = b;
            }
            
            return new ByteContents(arr);
        }
    }
    
    private record ShortContents(short[] Contents) : ILineData
    {
        public int BitWidth => sizeof(short) * 8;

        public int Line(int i) => Contents[i] << 16;
        
        public static ILineData Read(int i, List<byte> list)
        {
            var arr = new short[GlyphHeight];
            var j = 0;

            for (var k = 0; k < GlyphHeight; k++)
            {
                var l = DecodeHex(i, list, j++);
                var m = DecodeHex(i, list, j++);
                var n = DecodeHex(i, list, j++);
                var o = DecodeHex(i, list, j++);
                
                var b = (short) (l << 12 | m << 8 | n << 4 | o);
                arr[k] = b;
            }
            
            return new ShortContents(arr);
        }
    }
    
    private record IntContents(int[] Contents, int BitWidth) : ILineData
    {
        public int Line(int i) => Contents[i];
        
        public static ILineData Read24(int i, List<byte> list)
        {
            var arr = new int[GlyphHeight];
            var j = 0;
            var k = 0;

            for (var l = 0; l < GlyphHeight; l++)
            {
                var m = DecodeHex(i, list, k++);
                var n = DecodeHex(i, list, k++);
                var o = DecodeHex(i, list, k++);
                var p = DecodeHex(i, list, k++);
                var q = DecodeHex(i, list, k++);
                var r = DecodeHex(i, list, k++);
                
                var b = m << 20 | n << 16 | o << 12 | p << 8 | q << 4 | r;
                arr[k] = b << 8;
                j |= b;
            }

            return new IntContents(arr, 24);
        }
        
        public static ILineData Read32(int i, List<byte> list)
        {
            var arr = new int[GlyphHeight];
            var j = 0;
            var k = 0;

            for (var l = 0; l < GlyphHeight; l++)
            {
                var m = DecodeHex(i, list, k++);
                var n = DecodeHex(i, list, k++);
                var o = DecodeHex(i, list, k++);
                var p = DecodeHex(i, list, k++);
                var q = DecodeHex(i, list, k++);
                var r = DecodeHex(i, list, k++);
                var s = DecodeHex(i, list, k++);
                var t = DecodeHex(i, list, k++);
                
                var b = m << 28 | n << 24 | o << 20 | p << 16 | q << 12 | r << 8 | s << 4 | t;
                arr[k] = b;
                j |= b;
            }

            return new IntContents(arr, 32);
        }
    }

    public record Dimensions(int Left, int Right)
    {
        public static IMapCodec<Dimensions> MapCodec { get; } =
            RecordCodecBuilder.MapCodec<Dimensions>(instance => instance
                .Group(
                    Data.Codec.Int.FieldOf("left").ForGetter<Dimensions>(d => d.Left),
                    Data.Codec.Int.FieldOf("right").ForGetter<Dimensions>(d => d.Right))
                .Apply(instance, (l, r) => new Dimensions(l, r))
            );

        public static ICodec<Dimensions> Codec { get; } = MapCodec.Codec;

        public int Pack() => Pack(Left, Right);
        
        public static int Pack(int left, int right) => (left & 0xff) << 8 | right & 0xff;

        public static int UnpackLeft(int packed) => (byte) (packed >> 8);
        public static int UnpackRight(int packed) => (byte) packed;
    }

    private record OverrideRange(int From, int To, Dimensions Dimensions)
    {
        private static ICodec<OverrideRange> RawCodec { get; } =
            RecordCodecBuilder.Create<OverrideRange>(instance => instance
                .Group(
                    ExtraCodecs.Codepoint.FieldOf("from").ForGetter<OverrideRange>(o => o.From),
                    ExtraCodecs.Codepoint.FieldOf("to").ForGetter<OverrideRange>(o => o.To),
                    Dimensions.MapCodec.ForGetter<OverrideRange>(o => o.Dimensions))
                .Apply(instance, (f, t, d) => new OverrideRange(f, t, d))
            );

        public static ICodec<OverrideRange> Codec { get; } = ExtraCodecs.Validate(RawCodec, o =>
        {
            return o.From >= o.To
                ? DataResult.Error<OverrideRange>(() => $"Invalid range: [{o.From}; {o.To}]")
                : DataResult.Success(o);
        });
    }

    public class Definition : IGlyphProviderDefinition
    {
        public static IMapCodec<Definition> Codec { get; } =
            RecordCodecBuilder.MapCodec<Definition>(instance => instance
                .Group(
                    ResourceLocation.Codec.FieldOf("hex_file").ForGetter<Definition>(d => d.HexFile),
                    OverrideRange.Codec.ListCodec.FieldOf("size_overrides").ForGetter<Definition>(d => d._sizeOverrides)
                )
                .Apply(instance, (r, l) => new Definition(r, l))
            );
        
        private readonly List<OverrideRange> _sizeOverrides;
        
        public ResourceLocation HexFile { get; }

        private Definition(ResourceLocation hexFile, List<OverrideRange> sizeOverrides)
        {
            HexFile = hexFile;
            _sizeOverrides = sizeOverrides;
        }

        public GlyphProviderType Type => GlyphProviderType.Unihex;

        public IEither<IGlyphProviderDefinition.ILoader, IGlyphProviderDefinition.Reference> Unpack()
        {
            return Either.CreateLeft(IGlyphProviderDefinition.ILoader.Create(Load))
                .Right<IGlyphProviderDefinition.Reference>();
        }

        private IGlyphProvider Load(IResourceManager manager)
        {
            using var stream = manager.Open(HexFile);
            return LoadData(stream);
        }

        private UnihexProvider LoadData(Stream stream)
        {
            var codepointMap = new CodepointMap<ILineData>();
            var output = new ReaderOutput((i, data) => codepointMap.Put(i, data));
            var archive = new ZipArchive(stream);
            
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".hex"))
                {
                    Logger.Info($"Found {entry.FullName}, loading");
                    ReadFromStream(entry.Open(), output);
                }
            }

            var map2 = new CodepointMap<Glyph>();
            foreach (var (i, j, d) in _sizeOverrides)
            {
                var k = i;

                while (true)
                {
                    if (k > j) break;
                    
                    var lineData = codepointMap.Remove(k);
                    if (lineData != null)
                    {
                        map2.Put(k, new Glyph(lineData, d.Left, d.Right));
                    }

                    k++;
                }
            }
            
            foreach (var (index, lineData) in codepointMap)
            {
                var j = lineData.CalculateWidth();
                var k = Dimensions.UnpackLeft(j);
                var l = Dimensions.UnpackRight(j);
                map2.Put(index, new Glyph(lineData, k, l));
            }

            return new UnihexProvider(map2);
        }
    }
}