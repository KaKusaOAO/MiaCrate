using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Nbt;

namespace MiaCrate.Nbt;

public class NbtOps : IDynamicOps<NbtTag>
{
    public NbtOps()
    {
        MapBuilder = new NbtRecordBuilder(this);
    }
    
    public NbtTag Empty => NbtEnd.Shared;
    public NbtTag EmptyMap => new NbtCompound();
    public NbtTag EmptyList => new NbtList();

    public IDynamicOps<TOut> ConvertTo<TOut>(IDynamicOps<TOut> outOps, NbtTag input)
    {
        throw new NotImplementedException();
    }

    public NbtTag CreateString(string value) => new NbtString(value);

    public IRecordBuilder<NbtTag> MapBuilder { get; }
    public IDataResult<IEnumerable<NbtTag>> GetEnumerable(NbtTag input)
    {
        throw new NotImplementedException();
    }

    public NbtTag CreateList(IEnumerable<NbtTag> input)
    {
        throw new NotImplementedException();
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

    public NbtTag CreateByte(byte val) => new NbtByte(val);

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