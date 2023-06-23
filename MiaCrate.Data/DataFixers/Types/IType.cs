using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public interface IType : IApp
{
    public class Mu : IK1 {}

    public IBang CreateBang();
    public IType FindFieldType(string name) => 
        FindFieldTypeOpt(name).OrElse(() => throw new ArgumentException($"Field not found: {name}"));
    public IOptional<IType> FindFieldTypeOpt(string name) => Optional.Empty<IType>();
    public IType UpdateMu(RecursiveTypeFamily newFamily);
    public ITypeTemplate Template { get; }
    public ITypeTemplate BuildTemplate();
    public IOptional<IType> FindCheckedType(int index);
    public IOptional Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule);
    public bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex);
}

public interface IType<T> : IType, IApp<IType.Mu, T>
{
    public ICodec<T> Codec { get; }
    public IOptional<T> Point(IDynamicOps ops) => Optional.Empty<T>();
    public IDataResult<TDynamic> Write<TDynamic>(IDynamicOps<TDynamic> ops, T value);
    public IDataResult<IDynamic<TDynamic>> WriteDynamic<TDynamic>(IDynamicOps<TDynamic> ops, T value);
    public IRewriteResultLeft<T> All(ITypeRewriteRule rule, bool recursive, bool checkIndex);
    public IOptional<IRewriteResultLeft<T>> One(ITypeRewriteRule rule);
    
    public new IBang<T> CreateBang() => Functions.Bang(this);
    IBang IType.CreateBang() => CreateBang();
    
    public new IOptional<IRewriteResultLeft<T>> Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule);
    IOptional IType.Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule) => Rewrite(rule, fRule);
}

public abstract class DataType<T> : IType<T>
{
    private ITypeTemplate? _template;
    private ICodec<T>? _codec;

    public ITypeTemplate Template => _template ??= BuildTemplate();
    public ICodec<T> Codec => _codec ??= BuildCodec();

    public IType UpdateMu(RecursiveTypeFamily newFamily) => this;
    public IOptional<IType> FindCheckedType(int index) => Optional.Empty<IType>();

    public virtual IOptional<T> Point(IDynamicOps ops) => Optional.Empty<T>();

    public IDataResult<TDynamic> Write<TDynamic>(IDynamicOps<TDynamic> ops, T value) => 
        Codec.Encode(value, ops, ops.Empty);

    public IDataResult<IDynamic<TDynamic>> WriteDynamic<TDynamic>(IDynamicOps<TDynamic> ops, T value)
    {
        throw new NotImplementedException();
        // return Write(ops, value).Select(result =)
    }

    public virtual IRewriteResultLeft<T> All(ITypeRewriteRule rule, bool recursive, bool checkIndex) => 
        RewriteResult.Nop(this);

    public virtual IOptional<IRewriteResultLeft<T>> One(ITypeRewriteRule rule) => 
        Optional.Empty<IRewriteResultLeft<T>>();

    public IOptional<IRewriteResultLeft<T>> Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule)
    {
        throw new NotImplementedException();
    }

    public abstract ITypeTemplate BuildTemplate();
    protected abstract ICodec<T> BuildCodec();

#pragma warning disable CS0659
    public override bool Equals(object? obj) => 
#pragma warning restore CS0659
        ReferenceEquals(this, obj) || Equals(obj, false, true);

    public abstract bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex);
}

public static class DataType
{
    public static IType<T> Unbox<T>(IApp<IType.Mu, T> box) => (IType<T>)box;
}