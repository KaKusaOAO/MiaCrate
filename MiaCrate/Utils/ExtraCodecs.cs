using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Texts;

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

    public static readonly ICodec<IComponent> FlatComponent = Codec.String.CrossSelectMany(str =>
    {
        try
        {
            var node = JsonSerializer.Deserialize<JsonNode>(str);
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
            return DataResult.Success(JsonSerializer.Serialize(node));
        }
        catch (Exception ex)
        {
            return DataResult.Error<string>(() => ex.Message);
        }
    });

    public static ICodec<T> AdaptJsonSerializer<T>(Func<JsonNode, T> deserialize, Func<T, JsonNode> serialize)
    {
        return Json.CrossSelectMany(n =>
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
}