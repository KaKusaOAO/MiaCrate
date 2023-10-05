using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Nbt;

namespace MiaCrate.Nbt;

public class NbtOps : IDynamicOps<NbtTag>
{
    public static NbtOps Instance { get; } = new();
    
    private NbtOps()
    {
        MapBuilder = new NbtRecordBuilder(this);
    }
    
    public NbtTag Empty => NbtEnd.Instance;
    public NbtTag EmptyMap => new NbtCompound();
    public NbtTag EmptyList => new NbtList();

    private IDynamicOps<NbtTag> Boxed() => this;

    public TOut ConvertTo<TOut>(IDynamicOps<TOut> outOps, NbtTag input)
    {
        switch (input.Type)
        {
            case TagType.End:
                return outOps.Empty;
            case TagType.Byte:
                return outOps.CreateByte(input.As<NbtByte>().Value);
            case TagType.Short:
                return outOps.CreateShort(input.As<NbtShort>().Value);
            case TagType.Int:
                return outOps.CreateInt(input.As<NbtInt>().Value);
            case TagType.Long:
                return outOps.CreateLong(input.As<NbtLong>().Value);
            case TagType.Float:
                return outOps.CreateFloat(input.As<NbtFloat>().Value);
            case TagType.Double:
                return outOps.CreateDouble(input.As<NbtDouble>().Value);
            case TagType.ByteArray:
                throw new NotImplementedException();
            case TagType.String:
                return outOps.CreateString(input.As<NbtString>().Value);
            case TagType.List:
                return Boxed().ConvertList(outOps, input);
            case TagType.Compound:
                return Boxed().ConvertMap(outOps, input);
            case TagType.IntArray:
                throw new NotImplementedException();
            case TagType.LongArray:
                throw new NotImplementedException();
            default:
                throw new ArgumentException($"Unknown tag type: {input}");
        }
    }

    public NbtTag CreateString(string value) => NbtString.CreateValue(value);

    public IRecordBuilder<NbtTag> MapBuilder { get; }
    
    public IDataResult<IEnumerable<NbtTag>> GetEnumerable(NbtTag input)
    {
        throw new NotImplementedException();
    }

    public IDataResult<IEnumerable<IPair<NbtTag, NbtTag>>> GetMapValues(NbtTag input)
    {
        if (input is NbtCompound compound)
        {
            return DataResult.Success(compound.Keys
                .Select(s => Pair.Of(CreateString(s), compound[s]))
            );
        }

        return DataResult.Error<IEnumerable<IPair<NbtTag, NbtTag>>>(() => $"Not a map: {input}");
    }

    public NbtTag CreateList(IEnumerable<NbtTag> input)
    {
        throw new NotImplementedException();
    }

    public NbtTag CreateMap(IEnumerable<IPair<NbtTag, NbtTag>> map)
    {
        var compound = new NbtCompound();
        foreach (var pair in map)
        {
            compound[pair.First.As<NbtString>().Value] = pair.Second;
        }

        return compound;
    }

    public IDataResult<NbtTag> MergeToMap(NbtTag prefix, IMapLike<NbtTag> mapLike)
    {
        throw new NotImplementedException();
    }

    public IDataResult<NbtTag> MergeToList(NbtTag list, NbtTag value)
    {
        throw new NotImplementedException();
    }

    public NbtTag CreateNumeric(decimal val)
    {
        throw new NotImplementedException();
    }

    public NbtTag CreateByte(byte val) => NbtByte.CreateValue(val);

    public IDataResult<string> GetStringValue(NbtTag value)
    {
        if (value is NbtString b) return DataResult.Success(b.Value);
        return DataResult.Error<string>(() => "Unsupported string tag");
    }

    public IDataResult<decimal> GetNumberValue(NbtTag value)
    {
        if (value is NbtByte b) return DataResult.Success((decimal)b.Value);
        return DataResult.Error<decimal>(() => "Unsupported number tag");
    }
}