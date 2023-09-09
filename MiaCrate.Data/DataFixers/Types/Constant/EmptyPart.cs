using MiaCrate.Data.Codecs;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Data;

public sealed class EmptyPart : DataType<Unit>
{
    public override IOptional<Unit> Point(IDynamicOps ops) => Optional.Of(Unit.Instance);
    public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex) => ReferenceEquals(this, obj);
    public override ITypeTemplate BuildTemplate() => Dsl.ConstType(this);
    protected override ICodec<Unit> BuildCodec() => Data.Codec.Empty.Codec;
}