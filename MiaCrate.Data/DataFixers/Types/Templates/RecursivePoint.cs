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
        return new RecursivePointFamily(result);
    }

    private class RecursivePointFamily : ITypeFamily
    {
        private readonly IType _result;

        public RecursivePointFamily(IType result)
        {
            _result = result;
        }

        public IType Apply(int index) => _result;
    }
    
    public interface IRecursivePointType : IType
    {
        public RecursiveTypeFamily Family { get; }
        public int Index { get; }
    }
    
    public interface IRecursivePointType<T> : IRecursivePointType, IType<T>
    {
        
    }

    public class RecursivePointType<T> : DataType<T>, IRecursivePointType<T>
    {
        public RecursiveTypeFamily Family { get; }
        public int Index { get; }
        private readonly Func<IType<T>> _func;
        private IType<T>? _type;

        public RecursivePointType(RecursiveTypeFamily family, int index, Func<IType<T>> func)
        {
            Family = family;
            Index = index;
            _func = func;
        }

        public IType<T> Unfold() => _type ??= _func();

        protected override ICodec<T> BuildCodec() => new TypeCodec(this);
        public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex)
        {
            if (obj is not IRecursivePointType type) return false;
            return (ignoreRecursionPoints || Family == type.Family) && Index == type.Index;
        }

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

        public override ITypeTemplate BuildTemplate() => Dsl.Id(Index);
    }
    
    public class RecursivePointType : DataType<object>, IRecursivePointType
    {
        public RecursiveTypeFamily Family { get; }
        public int Index { get; }
        private readonly Func<IType> _func;
        private IType? _type;

        public RecursivePointType(RecursiveTypeFamily family, int index, Func<IType> func)
        {
            Family = family;
            Index = index;
            _func = func;
        }

        public IType Unfold() => _type ??= _func();

        protected override ICodec<object> BuildCodec() => new TypeCodec(this);
        public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex)
        {
            if (obj is not IRecursivePointType type) return false;
            return (ignoreRecursionPoints || Family == type.Family) && Index == type.Index;
        }

        private class TypeCodec : ICodec<object>
        {
            private readonly RecursivePointType _instance;

            public TypeCodec(RecursivePointType instance)
            {
                _instance = instance;
            }

            public IDataResult<TOut> Encode<TOut>(object input, IDynamicOps<TOut> ops, TOut prefix) =>
                _instance.Unfold().Codec.Encode(input, ops, prefix).SetLifecycle(Lifecycle.Experimental);
            
            public IDataResult<IPair<object, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
                _instance.Unfold().Codec.Decode(ops, input).SetLifecycle(Lifecycle.Experimental);
        }

        public override ITypeTemplate BuildTemplate() => Dsl.Id(Index);
    }
}