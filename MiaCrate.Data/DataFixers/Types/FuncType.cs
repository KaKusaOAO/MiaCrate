using System.Runtime.CompilerServices;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public interface IFuncType : IType<IFunction>
{
    public IType First { get; }
    public IType Second { get; }
}

public interface IFuncTypeIn<T> : IFuncType
{
    public new IType<T> First { get; }
    IType IFuncType.First => First;
}

public interface IFuncTypeOut<T> : IFuncType
{
    public new IType<T> Second { get; }
    IType IFuncType.Second => Second;
}

public interface IFuncType<TIn, TOut> : IType<IFunction<TIn, TOut>>, IFuncTypeIn<TIn>, IFuncTypeOut<TOut>
{
    public new ICodec<IFunction<TIn, TOut>> Codec { get; }
    ICodec<IFunction<TIn, TOut>> IType<IFunction<TIn, TOut>>.Codec => Codec;
    ICodec<IFunction> IType<IFunction>.Codec => 
        Codec.CrossSelect<IFunction>(e => e, e => (IFunction<TIn, TOut>)e);

    public new IBang<IFunction<TIn, TOut>> CreateBang() => Functions.Bang<IFunction<TIn, TOut>>(this);
    IBang<IFunction<TIn, TOut>> IType<IFunction<TIn, TOut>>.CreateBang() => CreateBang();
    IBang<IFunction> IType<IFunction>.CreateBang() => Unsafe.As<IBang<IFunction>>(CreateBang());
    
    public new IOptional<IFunction<TIn, TOut>> Point(IDynamicOps ops) => Optional.Empty<IFunction<TIn, TOut>>();
    IOptional<IFunction<TIn, TOut>> IType<IFunction<TIn, TOut>>.Point(IDynamicOps ops) => Point(ops);
    IOptional<IFunction> IType<IFunction>.Point(IDynamicOps ops) => Point(ops);
    
    public new IDataResult<TDynamic> Write<TDynamic>(IDynamicOps<TDynamic> ops, IFunction<TIn, TOut> value);
    IDataResult<TDynamic>
        IType<IFunction<TIn, TOut>>.Write<TDynamic>(IDynamicOps<TDynamic> ops, IFunction<TIn, TOut> value) =>
        Write(ops, value);
    IDataResult<TDynamic> IType<IFunction>.Write<TDynamic>(IDynamicOps<TDynamic> ops, IFunction value) =>
        Write(ops, (IFunction<TIn, TOut>) value);
    
    public new IDataResult<IDynamic<TDynamic>> WriteDynamic<TDynamic>(IDynamicOps<TDynamic> ops, IFunction<TIn, TOut> value);
    IDataResult<IDynamic<TDynamic>>
        IType<IFunction<TIn, TOut>>.WriteDynamic<TDynamic>(IDynamicOps<TDynamic> ops, IFunction<TIn, TOut> value) =>
        WriteDynamic(ops, value);
    IDataResult<IDynamic<TDynamic>> 
        IType<IFunction>.WriteDynamic<TDynamic>(IDynamicOps<TDynamic> ops, IFunction value) =>
        WriteDynamic(ops, (IFunction<TIn, TOut>) value);
    
    public new IRewriteResultLeft<IFunction<TIn, TOut>> All(ITypeRewriteRule rule, bool recursive, bool checkIndex);
    IRewriteResultLeft<IFunction<TIn, TOut>>
        IType<IFunction<TIn, TOut>>.All(ITypeRewriteRule rule, bool recursive, bool checkIndex) =>
        All(rule, recursive, checkIndex);
    IRewriteResultLeft<IFunction>
        IType<IFunction>.All(ITypeRewriteRule rule, bool recursive, bool checkIndex) =>
        Unsafe.As<IRewriteResultLeft<IFunction>>(All(rule, recursive, checkIndex));
    
    public new IOptional<IRewriteResultLeft<IFunction<TIn, TOut>>> One(ITypeRewriteRule rule);
    IOptional<IRewriteResultLeft<IFunction<TIn, TOut>>>
        IType<IFunction<TIn, TOut>>.One(ITypeRewriteRule rule) => One(rule);
    IOptional<IRewriteResultLeft<IFunction>>
        IType<IFunction>.One(ITypeRewriteRule rule) => 
        One(rule).Select(e => Unsafe.As<IRewriteResultLeft<IFunction>>(e));
    
    public new IOptional<IRewriteResultLeft<IFunction<TIn, TOut>>> Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule);
    IOptional IType.Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule) => Rewrite(rule, fRule);
    IOptional<IRewriteResultLeft<IFunction<TIn, TOut>>>
        IType<IFunction<TIn, TOut>>.Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule) =>
        Rewrite(rule, fRule);
    IOptional<IRewriteResultLeft<IFunction>> 
        IType<IFunction>.Rewrite(ITypeRewriteRule rule, IPointFreeRule fRule) => 
        Rewrite(rule, fRule).Select(e => Unsafe.As<IRewriteResultLeft<IFunction>>(e));
}

public class FuncType<TIn, TOut> : DataType<IFunction<TIn, TOut>>, IFuncType<TIn, TOut>
{
    public IType<TIn> First { get; }
    public IType<TOut> Second { get; }

    public FuncType(IType<TIn> first, IType<TOut> second)
    {
        First = first;
        Second = second;
    }
    
    public override ITypeTemplate BuildTemplate() =>
        throw new NotSupportedException("No template for function types.");

    protected override ICodec<IFunction<TIn, TOut>> BuildCodec() => Codecs.Codec.Of(
        Encoder.Error<IFunction<TIn, TOut>>("Cannot save a function"),
        Decoder.Error<IFunction<TIn, TOut>>("Cannot read a function")
    );

    public override int GetHashCode() => HashCode.Combine(First, Second);

    public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex)
    {
        if (obj is not IFuncType other) return false;
        return First.Equals(other.First, ignoreRecursionPoints, checkIndex) &&
               Second.Equals(other.Second, ignoreRecursionPoints, checkIndex);
    }
}