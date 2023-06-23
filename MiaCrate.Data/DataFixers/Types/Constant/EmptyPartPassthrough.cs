using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public class EmptyPartPassthrough : DataType<IDynamic>
{
    public override IOptional<IDynamic> Point(IDynamicOps ops) => Optional.Of(ops.CreateEmptyDynamic());

    public override ITypeTemplate BuildTemplate() => Dsl.ConstType(this);

    protected override ICodec<IDynamic> BuildCodec()
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex) => 
        ReferenceEquals(this, obj);
}