namespace MiaCrate.Data;

public interface ITypeFamily
{
    IType Apply(int index);
}

public static class TypeFamily
{
    public static IFamilyOptic<TLeft, TRight>
        FamilyOptic<TLeft, TRight>(Func<int, IOpticParts<TLeft, TRight>> optics) =>
        new PassThroughFamilyOptic<TLeft, TRight>(optics);

    public static ITypeFamily Create(Func<int, IType> func) => new Instance(func);
    
    private class PassThroughFamilyOptic<TLeft, TRight> : IFamilyOptic<TLeft, TRight>
    {
        private readonly Func<int, IOpticParts<TLeft, TRight>> _func;
        
        public PassThroughFamilyOptic(Func<int, IOpticParts<TLeft, TRight>> func)
        {
            _func = func;
        }

        public IOpticParts<TLeft, TRight> Apply(int index) => _func(index);
    }

    private class Instance : ITypeFamily
    {
        private readonly Func<int, IType> _func;

        public Instance(Func<int, IType> func)
        {
            _func = func;
        }

        public IType Apply(int index) => _func(index);
    }
}