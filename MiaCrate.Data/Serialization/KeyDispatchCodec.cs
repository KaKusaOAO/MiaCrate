using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public class KeyDispatchCodec<TKey, TValue> : MapCodec<TValue>
{
    private const string ValueKey = "value";
    
    private readonly string _typeKey;
    private readonly ICodec<TKey> _keyCodec;
    private readonly Func<TValue, IDataResult<TKey>> _type;
    private readonly Func<TKey, IDataResult<IDecoder<TValue>>> _decoder;
    private readonly Func<TValue, IDataResult<IEncoder<TValue>>> _encoder;
    private readonly bool _assumeMap;

    protected KeyDispatchCodec(string typeKey, ICodec<TKey> keyCodec, Func<TValue, IDataResult<TKey>> type,
        Func<TKey, IDataResult<IDecoder<TValue>>> decoder, Func<TValue, IDataResult<IEncoder<TValue>>> encoder,
        bool assumeMap)
    {
        _typeKey = typeKey;
        _keyCodec = keyCodec;
        _type = type;
        _decoder = decoder;
        _encoder = encoder;
        _assumeMap = assumeMap;
    }

    public KeyDispatchCodec(string typeKey, ICodec<TKey> keyCodec, Func<TValue, IDataResult<TKey>> type,
        Func<TKey, IDataResult<ICodec<TValue>>> codec)
        : this(typeKey, keyCodec, type, 
            v => codec(v).Select(c => (IDecoder<TValue>) c), 
            v => GetCodec(type, k => codec(k).Select(c => (IEncoder<TValue>) c), v), false)
    {
        
    }

    private static IDataResult<IEncoder<TValue>> GetCodec(Func<TValue, IDataResult<TKey>> type,
        Func<TKey, IDataResult<IEncoder<TValue>>> encoder, TValue input)
    {
        return type(input).SelectMany(encoder);
    }

    public override IEnumerable<T> GetKeys<T>(IDynamicOps<T> ops) => new[] {_typeKey, ValueKey}.Select(ops.CreateString);

    public override IRecordBuilder<TOut> Encode<TOut>(TValue input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix)
    {
        var encoder = _encoder(input);
        var builder = prefix.WithErrorsFrom(encoder);
        if (encoder.Result.IsPresent) return builder;

        var c = encoder.Result.Value;
        if (ops.CompressMaps)
        {
            return prefix
                .Add(_typeKey, _type(input).SelectMany(t => _keyCodec.EncodeStart(ops, t)))
                .Add(ValueKey, c.EncodeStart(ops, input));
        }

        if (c is IMapCodecCodec<TValue> codecCodec)
        {
            return codecCodec.Codec.Encode(input, ops, prefix)
                .Add(_typeKey, _type(input).SelectMany(t => _keyCodec.EncodeStart(ops, t)));
        }

        var typeStr = ops.CreateString(_typeKey);
        var result = c.EncodeStart(ops, input);

        if (_assumeMap)
        {
            var element = result.SelectMany(ops.GetMap);
            return element.Select(map =>
            {
                prefix.Add(typeStr, _type(input).SelectMany(t => _keyCodec.EncodeStart(ops, t)));
                foreach (var pair in map.Entries)
                {
                    if (!pair.First!.Equals(typeStr))
                        prefix.Add(pair.First!, pair.Second!);
                }

                return prefix;
            }).Result.OrElseGet(() => prefix.WithErrorsFrom(element));
        }

        prefix.Add(typeStr, _type(input).SelectMany(t => _keyCodec.EncodeStart(ops, t)));
        prefix.Add(ValueKey, result);
        return prefix;
    }

    public override IDataResult<TValue> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input)
    {
        var name = input[_typeKey];
        if (name == null)
            return DataResult.Error<TValue>(() => $"Input does not contain a key [{_typeKey}]: {input}");

        return _keyCodec.Decode(ops, name).SelectMany(type =>
        {
            var decoder = _decoder(type.First!);
            return decoder.SelectMany(c =>
            {
                if (ops.CompressMaps)
                {
                    var value = input[ops.CreateString(ValueKey)];
                    if (value == null)
                    {
                        return DataResult.Error<TValue>(() => $"Input does not have a \"{ValueKey}\" entry: {input}");
                    }

                    return c.Parse(ops, value);
                }

                if (c is IMapCodecCodec<TValue> codecCodec)
                {
                    return codecCodec.Codec.Decode(ops, input);
                }

                if (_assumeMap)
                {
                    return c.Decode(ops, ops.CreateMap(input.Entries)).Select(r => r.First!);
                }

                return c.Decode(ops, input[ValueKey]!).Select(r => r.First!);
            });
        });
    }
}