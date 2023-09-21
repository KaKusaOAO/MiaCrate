using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate;

public static class ExtraCodecs
{
    public static readonly ICodec<JsonNode> Json = Codec.Passthrough.CrossSelect(
        d => d.Convert(JsonOps.Instance).Value,
        n => new Dynamic<JsonNode>(JsonOps.Instance, n)
    );

    public static readonly ICodec<IComponent> Component = AdaptJsonSerializer(
        Mochi.Texts.Component.FromJson,
        x => x.ToJson()
    );

    public static readonly ICodec<IComponent> FlatComponent = Codec.String.FlatCrossSelect(str =>
    {
        try
        {
            var node = JsonNode.Parse(str);
            return DataResult.Success(Mochi.Texts.Component.FromJson(node));
        }
        catch (Exception ex)
        {
            return DataResult.Error<IComponent>(() => ex.Message);
        }
    }, comp =>
    {
        try
        {
            var node = comp.ToJson();
            return DataResult.Success(node.ToJsonString());
        }
        catch (Exception ex)
        {
            return DataResult.Error<string>(() => ex.Message);
        }
    });

    public static readonly ICodec<Regex> Regex = Codec.String.CoSelectSelectMany(str =>
    {
        try
        {
            return DataResult.Success(new Regex(str));
        }
        catch (ArgumentException ex)
        {
            return DataResult.Error<Regex>(() =>
                $"Invalid regex '{str}': {ex.Message}");
        }
    }, r => r.ToString());

    public static ICodec<T> AdaptJsonSerializer<T>(Func<JsonNode, T> deserialize, Func<T, JsonNode> serialize)
    {
        return Json.FlatCrossSelect(n =>
        {
            try
            {
                return DataResult.Success(deserialize(n));
            }
            catch (Exception ex)
            {
                return DataResult.Error<T>(() => ex.Message);
            }
        }, obj =>
        {
            try
            {
                return DataResult.Success(serialize(obj));
            }
            catch (Exception ex)
            {
                return DataResult.Error<JsonNode>(() => ex.Message);
            }
        });
    }

    public static ICodec<T> WithAlternative<T, TLeftCompatible>(ICodec<T> a, ICodec<TLeftCompatible> b)
        where TLeftCompatible : T =>
        Codec.Either(a, b)
            .CrossSelect(e => e.Select(x => x, x => x), Either.Left<T, TLeftCompatible>);
    
    public static ICodec<T> WithAlternative<T, TOther>(ICodec<T> a, ICodec<TOther> b, Func<TOther, T> convert) =>
        Codec.Either(a, b)
            .CrossSelect(e => e.Select(x => x, convert), Either.Left<T, TOther>);

    public static ICodec<T> IntervalCodec<TElement, T>(ICodec<TElement> codec, 
        string firstElementName, string secondElementName,
        Func<TElement, TElement, IDataResult<T>> createFromElements, 
        Func<T, TElement> firstElement, 
        Func<T, TElement> secondElement)
    {
        var listCodec = Codec.ListOf(codec)
            .CoSelectSelectMany(x => Util.FixedSize(x, 2).SelectMany(list =>
            {
                var first = list[0];
                var second = list[1];
                return createFromElements(first, second);
            }), obj => new List<TElement>
            {
                firstElement(obj),
                secondElement(obj)
            });
        
        var recordCodec = RecordCodecBuilder.Create<IPair<TElement, TElement>>(instance => instance
            .Group(
                codec.FieldOf(firstElementName).ForGetter<IPair<TElement, TElement>>(x => x.First!),
                codec.FieldOf(secondElementName).ForGetter<IPair<TElement, TElement>>(x => x.Second!)
            )
            .Apply(instance, Pair.Of)
        )
            .CoSelectSelectMany(
                pair => createFromElements(pair.First!, pair.Second!),
                obj => Pair.Of(firstElement(obj), secondElement(obj))
            );

        var alternative = WithAlternative(listCodec, recordCodec);
        
        return Codec.Either(codec, alternative).CoSelectSelectMany(
            e => e.Select(
                element => createFromElements(element, element),
                DataResult.Success
            ),
            obj =>
            {
                var first = firstElement(obj);
                var second = secondElement(obj);
                return first!.Equals(second) ? Either.Left<TElement, T>(first) : Either.Right<TElement, T>(obj);
            });
    }

    public static ICodec<T> Validate<T>(ICodec<T> codec, Func<T, IDataResult<T>> validate)
    {
        if (codec is IMapCodecCodec<T> codecCodec)
            return Validate(codecCodec.Codec, validate).Codec;

        return codec.FlatCrossSelect(validate, validate);
    }

    public static IMapCodec<T> Validate<T>(IMapCodec<T> mapCodec, Func<T, IDataResult<T>> validate) => 
        mapCodec.FlatCrossSelect(validate, validate);

    public static ICodec<T> OrCompressed<T>(ICodec<T> uncompressed, ICodec<T> compressed) =>
        new OrCompressedCodec<T>(uncompressed, compressed);

    public static ICodec<T> StringResolverCodec<T>(Func<T, string> func, Func<string, T?> resolver)
        where T : class =>
        Codec.String.FlatCrossSelect(
            str => Optional.OfNullable(resolver(str))
                .Select(DataResult.Success)
                .OrElseGet(() => DataResult.Error<T>(() => $"Unknown element name: {str}")),
            obj => Optional.OfNullable(func(obj))
                .Select(DataResult.Success)
                .OrElseGet(() => DataResult.Error<string>(() => $"Element with unknown name: {obj}"))
        );
    
    public static ICodec<T> IdResolverCodec<T>(Func<T, int> func, Func<int, T?> resolver, int i)
        where T : class =>
        Codec.Int.FlatCrossSelect(
            str => Optional.OfNullable(resolver(str))
                .Select(DataResult.Success)
                .OrElseGet(() => DataResult.Error<T>(() => $"Unknown element name: {str}")),
            obj =>
            {
                var j = func(obj);
                return j == i
                    ? DataResult.Error<int>(() => $"Element with unknown id: {obj}")
                    : DataResult.Success(j);
            });

    private class OrCompressedCodec<T> : ICodec<T>
    {
        private readonly ICodec<T> _uncompressed;
        private readonly ICodec<T> _compressed;

        public OrCompressedCodec(ICodec<T> uncompressed, ICodec<T> compressed)
        {
            _uncompressed = uncompressed;
            _compressed = compressed;
        }
        
        public IDataResult<TDynamic> Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix)
        {
            return ops.CompressMaps
                ? _compressed.Encode(input, ops, prefix)
                : _uncompressed.Encode(input, ops, prefix);
        }

        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
        {
            return ops.CompressMaps
                ? _compressed.Decode(ops, input)
                : _uncompressed.Decode(ops, input);
        }

        public override string ToString() => $"{_uncompressed} orCompressed {_compressed}";
    }
}