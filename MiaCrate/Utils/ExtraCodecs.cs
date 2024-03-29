﻿using System.Text;
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
    public static ICodec<JsonNode> Json { get; } = Codec.Passthrough.CrossSelect(
        d => d.Convert(JsonOps.Instance).Value,
        n => new Dynamic<JsonNode>(JsonOps.Instance, n)
    );

    public static ICodec<IComponent> Component { get; } = AdaptJsonSerializer(
        Mochi.Texts.Component.FromJson,
        x => x.ToJson()
    );

    public static ICodec<IComponent> FlatComponent { get; } = Codec.String.FlatCrossSelect(str =>
    {
        try
        {
            var node = JsonNode.Parse(str!);
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

    public static ICodec<int> NonNegativeInt { get; } = IntRangeWithMessage(0, int.MaxValue, 
        i => $"Value must be non-negative: {i}");
    
    public static ICodec<int> PositiveInt { get; } = IntRangeWithMessage(1, int.MaxValue, 
        i => $"Value must be positive: {i}");

    public static ICodec<Regex> Regex { get; } = Codec.String.CoSelectSelectMany(str =>
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

    public static ICodec<int> Codepoint { get; } = Codec.String.CoSelectSelectMany(s =>
    {
        var arr = Encoding.UTF32.GetBytes(s)
            .Chunk(4)
            .Select(arr => BitConverter.ToInt32(arr)).ToArray();

        return arr.Length != 1
            ? DataResult.Error<int>(() => $"Expected one codepoint, got: {s}")
            : DataResult.Success(arr[0]);
    }, char.ConvertFromUtf32);

    public static ICodec<TextColor> TextColor { get; } = Codec.String.CoSelectSelectMany(s =>
    {
        if (s.ToLowerInvariant() == "light_purple")
        {
            // TextColor defines this color as "purple"
            s = "purple";
        }

        try
        {
            return DataResult.Success(Mochi.Texts.TextColor.Of(s));
        }
        catch (Exception ex)
        {
            return DataResult.Error<TextColor>(() => "String is not a valid color name or hex color code");
        }
    }, color => color == Mochi.Texts.TextColor.Purple ? "light_purple" : color.Name);

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

    public static IMapCodec<IOptional<T>> StrictOptionalField<T>(ICodec<T> codec, string name) => 
        new StrictOptionalFieldCodec<T>(name, codec);

    public static IMapCodec<T> StrictOptionalField<T>(ICodec<T> codec, string name, T obj)
    {
        return StrictOptionalField(codec, name).CrossSelect(
            o => o.OrElse(obj),
            o => Equals(obj, o) ? Optional.Empty<T>() : Optional.Of(o));
    } 

    public static ICodec<T> Validate<T>(ICodec<T> codec, Func<T, IDataResult<T>> validate)
    {
        if (codec is IMapCodecCodec<T> codecCodec)
            return Validate(codecCodec.Codec, validate).Codec;

        return codec.FlatCrossSelect(validate, validate);
    }

    public static IMapCodec<T> Validate<T>(IMapCodec<T> mapCodec, Func<T, IDataResult<T>> validate) => 
        mapCodec.FlatCrossSelect(validate, validate);

    private static ICodec<int> IntRangeWithMessage(int min, int max, Func<int, string> message)
    {
        return Validate(Codec.Int, i =>
        {
            return i.CompareTo(min) >= 0 && i.CompareTo(max) <= 0
                ? DataResult.Success(i)
                : DataResult.Error<int>(() => message(i));
        });
    }

    public static ICodec<int> IntRange(int min, int max) => 
        IntRangeWithMessage(min, max, i => $"Value must be within range [{min};{max}]: {i}");

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
    
    public static ICodec<T> StringResolverCodec<T>(Func<T, string> func, Func<string, T?> resolver)
        where T : struct =>
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
    
    public static ICodec<T> IdResolverCodec<T>(Func<T, int> func, Func<int, T?> resolver, int i)
        where T : struct =>
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

    public static ICodec<T>
        OverrideLifecycle<T>(ICodec<T> codec, Func<T, Lifecycle> apply, Func<T, Lifecycle> coApply) =>
        codec.SelectResult(new OverrideLifecycleResultFunction<T>(apply, coApply));

    private class OverrideLifecycleResultFunction<T> : ICodec<T>.IResultFunction
    {
        private readonly Func<T, Lifecycle> _apply;
        private readonly Func<T, Lifecycle> _coApply;

        public OverrideLifecycleResultFunction(Func<T, Lifecycle> apply, Func<T, Lifecycle> coApply)
        {
            _apply = apply;
            _coApply = coApply;
        }

        public IDataResult<IPair<T, TOps>> Apply<TOps>(IDynamicOps<TOps> ops, TOps input, IDataResult<IPair<T, TOps>> a) => 
            a.Result.Select(p => a.SetLifecycle(_apply(p.First!))).OrElse(a);

        public IDataResult<TOps> CoApply<TOps>(IDynamicOps<TOps> ops, T input, IDataResult<TOps> t) => 
            t.SetLifecycle(_coApply(input));
    }

    private sealed class StrictOptionalFieldCodec<T> : MapCodec<IOptional<T>>
    {
        private readonly string _name;
        private readonly ICodec<T> _elementCodec;

        public StrictOptionalFieldCodec(string name, ICodec<T> elementCodec)
        {
            _name = name;
            _elementCodec = elementCodec;
        }

        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => Enumerable.Repeat(ops.CreateString(_name), 1);
        
        public override IRecordBuilder<TOut> Encode<TOut>(IOptional<T> input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix)
        {
            return input.IsPresent
                ? prefix.Add(_name, _elementCodec.EncodeStart(ops, input.Value))
                : prefix;
        }

        public override IDataResult<IOptional<T>> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input)
        {
            var obj = input[_name];
            return obj == null
                ? DataResult.Success(Optional.Empty<T>())
                : _elementCodec.Parse(ops, obj).Select(Optional.Of);
        }
    }

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