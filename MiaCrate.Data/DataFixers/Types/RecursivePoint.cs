using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public sealed class RecursivePoint : ITypeTemplate
{
    public int Index { get; }
    public int Size => Index + 1;

    public RecursivePoint(int index)
    {
        Index = index;
    }
    
    public ITypeFamily Apply(ITypeFamily family)
    {
        var result = family.Apply(Index);
        return new Family(result);
    }

    private class Family : ITypeFamily
    {
        private readonly IType _result;

        public Family(IType result)
        {
            _result = result;
        }

        public IType Apply(int index) => _result;
    }
    
    public interface IRecursivePointType : IType
    {
        
    }
    
    public interface IRecursivePointType<T> : IRecursivePointType, IType<T>
    {
        
    }

    public class RecursivePointType<T> : DataType<T>, IRecursivePointType<T>
    {
        public RecursiveTypeFamily Family { get; }
        public int Index1 { get; }
        private readonly Func<IType<T>> _func;
        private IType<T>? _type;

        private RecursivePointType(RecursiveTypeFamily family, int index, Func<IType<T>> func)
        {
            Family = family;
            Index1 = index;
            _func = func;
        }

        public IType<T> Unfold() => _type ??= _func();

        protected override ICodec<T> BuildCodec() => new TypeCodec(this);

        private class TypeCodec : ICodec<T>
        {
            private readonly RecursivePointType<T> _instance;

            public TypeCodec(RecursivePointType<T> instance)
            {
                _instance = instance;
            }

            public IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix) =>
                _instance.Unfold().Codec.Encode(input, ops, prefix).SetLifecycle(Lifecycle.Experimental);
            
            public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
                _instance.Unfold().Codec.Decode(ops, input).SetLifecycle(Lifecycle.Experimental);
        }

        public override ITypeTemplate BuildTemplate() => Dsl.Id(Index1);
    }
}