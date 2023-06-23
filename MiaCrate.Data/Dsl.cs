using Mochi.Core;

namespace MiaCrate.Data;

public static class Dsl
{
    public interface ITypeReference
    {
        string TypeName { get; }
    }

    private class TypeRef : ITypeReference
    {
        private readonly Func<string> _func;

        public TypeRef(Func<string> func)
        {
            _func = func;
        }

        public string TypeName => _func();
    }

    public static IType<Unit> EmptyPartType => Instances.EmptyPart;

    public static ITypeReference CreateReference(Func<string> func) => new TypeRef(func);
    public static ITypeTemplate ConstType(IType type) => new Const(type);
    public static ITypeTemplate Check(string name, int index, ITypeTemplate element) => 
        new Check(name, index, element);

    public static ITypeTemplate Named(string name, ITypeTemplate element)
    {
        throw new NotImplementedException();
    }

    public static ITypeTemplate Or(ITypeTemplate left, ITypeTemplate right)
    {
        throw new NotImplementedException();
    }

    public static ITypeTemplate Id(int index) => new RecursivePoint(index);

    public static IType<IFunction<TLeft, TRight>> Func<TLeft, TRight>(IType<TLeft> input, IType<TRight> output) => 
        new FuncType<TLeft, TRight>(input, output);

    private static class Instances
    {
        public static readonly IType<Unit> EmptyPart = new EmptyPart();
    }
}