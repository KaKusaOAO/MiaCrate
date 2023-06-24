using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Nbt;

namespace MiaCrate.Nbt;

public class NbtRecordBuilder : AbstractStringBuilder<NbtTag, NbtCompound>
{
    public NbtRecordBuilder(IDynamicOps<NbtTag> ops, Func<NbtCompound>? builder = null) : base(ops, builder)
    {
    }

    protected override IDataResult<NbtTag> Build(NbtCompound builder, NbtTag prefix) => 
        DataResult.Success<NbtTag>(builder);

    protected override NbtCompound InitBuilder() => new();

    protected override NbtCompound Append(string key, NbtTag value, NbtCompound builder)
    {
        builder[key] = value;
        return builder;
    }
}