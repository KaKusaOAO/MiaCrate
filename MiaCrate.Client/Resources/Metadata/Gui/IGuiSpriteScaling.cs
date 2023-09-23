using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Client.Resources;

public interface IGuiSpriteScaling
{
    public static ICodec<IGuiSpriteScaling> Codec { get; } =
        ScalingType.Codec.Dispatch(t => t.Type, t => t.ElementCodec);

    public static IGuiSpriteScaling Default { get; } = new TypeStretch();
    
    public ScalingType Type { get; }
    
    public sealed class ScalingType : IEnumLike<ScalingType>, IStringRepresentable
    {
        private static readonly Dictionary<int, ScalingType> _values = new();

        public static ScalingType Stretch { get; } = new("stretch", TypeStretch.Codec.Cast<IGuiSpriteScaling>());
        public static ScalingType Tile { get; } = new("tile", TypeTile.Codec.Cast<IGuiSpriteScaling>());
        public static ScalingType NineSlice { get; } = new("nine_slice", TypeNineSlice.Codec.Cast<IGuiSpriteScaling>());

        public static ICodec<ScalingType> Codec { get; } = IStringRepresentable.FromEnum(_values.Values.ToArray);
        
        public int Ordinal { get; }
        public string SerializedName { get; }
        public ICodec<IGuiSpriteScaling> ElementCodec { get; }

        private ScalingType(string key, ICodec<IGuiSpriteScaling> codec)
        {
            SerializedName = key;
            ElementCodec = codec;

            Ordinal = _values.Count;
            _values[Ordinal] = this;
        }
    }

    public sealed class TypeStretch : IGuiSpriteScaling
    {
        public static ICodec<TypeStretch> Codec { get; } = Data.Codec.Unit(() => new TypeStretch());
        
        public ScalingType Type => ScalingType.Stretch;
    }
    
    public record TypeNineSlice(int Width, int Height, TypeNineSlice.BorderData Border) : IGuiSpriteScaling
    {
        public static ICodec<TypeNineSlice> Codec { get; } = ExtraCodecs.Validate(
            RecordCodecBuilder.Create<TypeNineSlice>(instance => instance
                .Group(
                    ExtraCodecs.PositiveInt.FieldOf("width").ForGetter<TypeNineSlice>(t => t.Width),
                    ExtraCodecs.PositiveInt.FieldOf("height").ForGetter<TypeNineSlice>(t => t.Height),
                    BorderData.Codec.FieldOf("border").ForGetter<TypeNineSlice>(t => t.Border)
                )
                .Apply(instance, (w, h, b) => new TypeNineSlice(w, h, b))
            ), 
            Validate
        );
        
        public ScalingType Type => ScalingType.NineSlice;

        private static IDataResult<TypeNineSlice> Validate(TypeNineSlice data)
        {
            var border = data.Border;
            if (border.Left + border.Right >= data.Width)
            {
                return DataResult.Error<TypeNineSlice>(
                    () =>  $"Nine-sliced texture has no horizontal center slice: {border.Left} + {border.Right} >= {data.Width}"
                );
            }
            
            if (border.Top + border.Bottom >= data.Height)
            {
                return DataResult.Error<TypeNineSlice>(
                    () =>  $"Nine-sliced texture has no vertical center slice: {border.Top} + {border.Bottom} >= {data.Height}"
                );
            }

            return DataResult.Success(data);
        }

        public record BorderData(int Left, int Top, int Right, int Bottom)
        {
            private static readonly ICodec<BorderData> _valueCodec = ExtraCodecs.PositiveInt.FlatCoSelectSelect(
                i => new BorderData(i, i, i, i),
                b =>
                {
                    var o = b.UnpackValue();
                    return o.IsPresent
                        ? DataResult.Success(o.Value)
                        : DataResult.Error<int>(() => "Border has different side sizes");
                });

            private static readonly ICodec<BorderData> _recordCodec =
                RecordCodecBuilder.Create<BorderData>(instance => instance
                    .Group(
                        ExtraCodecs.NonNegativeInt.FieldOf("left").ForGetter<BorderData>(b => b.Left),    
                        ExtraCodecs.NonNegativeInt.FieldOf("top").ForGetter<BorderData>(b => b.Top),    
                        ExtraCodecs.NonNegativeInt.FieldOf("right").ForGetter<BorderData>(b => b.Right),    
                        ExtraCodecs.NonNegativeInt.FieldOf("bottom").ForGetter<BorderData>(b => b.Bottom)
                    )
                    .Apply(instance, (l, t, r, b) => new BorderData(l, t, r, b))
                );

            public static ICodec<BorderData> Codec { get; } = Data.Codec
                .Either(_valueCodec, _recordCodec).CrossSelect(
                    e => e.Select(n => n, n => n),
                    b => b.UnpackValue().IsPresent
                        ? Either.Left<BorderData, BorderData>(b)
                        : Either.Right<BorderData, BorderData>(b)
                );

            private IOptional<int> UnpackValue()
            {
                return Left == Top && Top == Right && Right == Bottom
                    ? Optional.Of(Left)
                    : Optional.Empty<int>();
            }
        }
    }
    
    public record TypeTile(int Width, int Height) : IGuiSpriteScaling
    {
        public static ICodec<TypeTile> Codec { get; } = RecordCodecBuilder.Create<TypeTile>(instance => instance
            .Group(
                ExtraCodecs.PositiveInt.FieldOf("width").ForGetter<TypeTile>(t => t.Width),
                ExtraCodecs.PositiveInt.FieldOf("height").ForGetter<TypeTile>(t => t.Height)
            )
            .Apply(instance, (w, h) => new TypeTile(w, h))
        );

        public ScalingType Type => ScalingType.Tile;
    }
}