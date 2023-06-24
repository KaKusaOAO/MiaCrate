namespace MiaCrate.Data.Codecs;

public interface IFieldEncoder : IMapEncoder
{
    
}

public interface IFieldEncoder<T> : IFieldEncoder, IMapEncoder<T>
{
    
}

public class FieldEncoder<T> : MapEncoder.Implementation<T>, IFieldEncoder<T>
{
    private readonly string _name;
    private readonly IEncoder<T> _elementCodec;

    public FieldEncoder(string name, IEncoder<T> elementCodec)
    {
        _name = name;
        _elementCodec = elementCodec;
    }

    public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => new[] { ops.CreateString(_name) };

    public override IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
        prefix.Add(_name, _elementCodec.EncodeStart(ops, input));
}