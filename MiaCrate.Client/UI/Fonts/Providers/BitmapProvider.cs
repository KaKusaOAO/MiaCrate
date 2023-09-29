using System.Text;
using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class BitmapProvider : IGlyphProvider
{
    private readonly NativeImage _image;
    private readonly CodepointMap<Glyph> _glyphs;

    private BitmapProvider(NativeImage image, CodepointMap<Glyph> glyphs)
    {
        _image = image;
        _glyphs = glyphs;
    }

    public ISet<int> GetSupportedGlyphs() => _glyphs.Keys.ToHashSet();
    
    public IGlyphInfo? GetGlyph(int id) => _glyphs.Get(id);

    public record Definition
        (ResourceLocation File, int Height, int Ascent, int[][] CodepointGrid) : IGlyphProviderDefinition
    {
        private static readonly ICodec<int[][]> _codepointGridCodec = ExtraCodecs.Validate(
            Data.Codec.String.ListCodec.CrossSelect(list =>
            {
                var i = list.Count;
                var grid = new int[i][];

                for (var j = 0; j < i; j++)
                {
                    var str = list[j];
                    grid[j] = Encoding.UTF32.GetBytes(str)
                        .Chunk(4)
                        .Select(arr => BitConverter.ToInt32(arr)).ToArray();
                }

                return grid;
            }, grid =>
            {
                var list = new List<string>();
                foreach (var arr in grid)
                {
                    var str = string.Join("", arr.Select(char.ConvertFromUtf32));
                    list.Add(str);
                }

                return list;
            }), ValidateDimensions);

        public static IMapCodec<Definition> Codec { get; } = ExtraCodecs.Validate(
            RecordCodecBuilder.MapCodec<Definition>(instance => instance
                .Group(
                    ResourceLocation.Codec.FieldOf("file").ForGetter<Definition>(d => d.File),
                    Data.Codec.Int.OptionalFieldOf("height", 8).ForGetter<Definition>(d => d.Height),
                    Data.Codec.Int.FieldOf("ascent").ForGetter<Definition>(d => d.Ascent),
                    _codepointGridCodec.FieldOf("chars").ForGetter<Definition>(d => d.CodepointGrid)
                )
                .Apply(instance, (l, h, a, c) => new Definition(l, h, a, c))
            ), Validate);

        private static IDataResult<int[][]> ValidateDimensions(int[][] grid)
        {
            var i = grid.Length;
            if (i == 0)
                return DataResult.Error<int[][]>(() => "Expected to find data in codepoint grid");

            var arr = grid[0];
            var j = arr.Length;
            if (j == 0)
                return DataResult.Error<int[][]>(() => "Expected to find data in codepoint grid");

            for (var k = 1; k < i; k++)
            {
                var row = grid[k];
                if (row.Length != j)
                    return DataResult.Error<int[][]>(() =>
                        "Lines in codepoint grid have to be the same length " +
                        $"(found: {row.Length} codepoints, expected: {j}), pad with \\u0000");
            }

            return DataResult.Success(grid);
        }

        private static IDataResult<Definition> Validate(Definition definition)
        {
            return definition.Ascent > definition.Height
                ? DataResult.Error<Definition>(() =>
                    $"Ascent {definition.Ascent} higher than height {definition.Height}")
                : DataResult.Success(definition);
        }

        public GlyphProviderType Type => GlyphProviderType.Bitmap;

        public IEither<IGlyphProviderDefinition.ILoader, IGlyphProviderDefinition.Reference> Unpack() => Either
            .CreateLeft(IGlyphProviderDefinition.ILoader.Create(Load))
            .Right<IGlyphProviderDefinition.Reference>();

        private IGlyphProvider Load(IResourceManager manager)
        {
            var location = File.WithPrefix("textures/");
            using var stream = manager.Open(location);
            var image = NativeImage.Read(stream);
            var texWidth = image.Width;
            var texHeight = image.Height;
            var width = texWidth / CodepointGrid[0].Length;
            var height = texHeight / CodepointGrid.Length;

            var hRatio = (float) Height / height;
            var codepointMap = new CodepointMap<Glyph>();

            for (var row = 0; row < CodepointGrid.Length; row++)
            {
                var n = 0;
                foreach (var codepoint in CodepointGrid[row])
                {
                    var col = n++;
                    
                    // Skip null characters
                    if (codepoint == 0) continue;
                    
                    var glyphWidth = GetActualGlyphWidth(image, width, height, col, row);
                    var glyph = codepointMap.Put(codepoint,
                        new Glyph(hRatio, image, col * width, row * height, width, height, (int) (0.5 + glyphWidth * hRatio) + 1, Ascent));
                    if (glyph != null)
                    {
                        Logger.Warn($"Codepoint '{codepoint:x}' declared multiple times in {location}");
                    }
                }
            }

            return new BitmapProvider(image, codepointMap);
        }

        private int GetActualGlyphWidth(NativeImage image, int width, int height, int col, int row)
        {
            var m = width - 1;
            for (; m >= 0; m--)
            {
                var x = col * width + m;

                for (var o = 0; o < height; o++)
                {
                    var y = row * height + o;
                    if (image.GetLuminanceOrAlpha(x, y) != 0)
                    {
                        return m + 1;
                    }
                }
            }

            return m + 1;
        }
    }

    private record Glyph(float Scale, NativeImage Image, int OffsetX, int OffsetY, int Width, int Height, int Advance,
        int Ascent) : IGlyphInfo
    {
        float IGlyphInfo.Advance => Advance;
        
        public BakedGlyph Bake(Func<ISheetGlyphInfo, BakedGlyph> func) => func(new Info(this));

        private class Info : ISheetGlyphInfo
        {
            private readonly Glyph _instance;
            
            public float Oversample => 1f / _instance.Scale;
            public int PixelWidth => _instance.Width;
            public int PixelHeight => _instance.Height;
            public float BearingY => ISheetGlyphInfo.DefaultBearingY + 7f - _instance.Ascent;
            public bool IsColored => _instance.Image.Format.Components > 1;
            
            public Info(Glyph instance)
            {
                _instance = instance;
            }

            public void Upload(AbstractTexture texture, int x, int y)
            {
                // instance image is the font texture in the resource pack
                // the texture argument is the font atlas
                // x and y stands for the position in the font atlas (dynamic one)
                // offsetX and offsetY stand for the position in the source texture
                _instance.Image.Upload(texture, 0, x, y, _instance.OffsetX, _instance.OffsetY, 
                    _instance.Width, _instance.Height, false, false);
            }
        }
    }
}