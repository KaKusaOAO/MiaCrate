using MiaCrate.Data.Codecs;
using Mochi.Core;
using Mochi.Utils;

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

    public static ITypeTemplate Remainder() => ConstType(Instances.EmptyPassthrough);

    public static ITypeTemplate Named(string name, ITypeTemplate element) => new Named(name, element);
    public static IType Named(string name, IType element) => new Named.NamedType(name, element);

    public static ITypeTemplate Or(ITypeTemplate left, ITypeTemplate right)
    {
        throw new NotImplementedException();
    }

    public static ITypeTemplate Id(int index) => new RecursivePoint(index);

    public static IType<IFunction<TLeft, TRight>> Func<TLeft, TRight>(IType<TLeft> input, IType<TRight> output) => 
        new FuncType<TLeft, TRight>(input, output);

    private static class Instances
    { 
        public static IType<Unit> EmptyPart { get; } = new EmptyPart();
        public static IType<IDynamic> EmptyPassthrough { get; } = new EmptyPartPassthrough();
    }
}

public record Named(string Name, ITypeTemplate Element) : ITypeTemplate
{
    public int Size => Element.Size;

    public ITypeFamily Apply(ITypeFamily family) => TypeFamily.Create(i => Dsl.Named(Name, Element.Apply(family).Apply(i)));

    public class NamedType : DataType<IPair<string, object>>
    {
        private readonly string _name;
        private readonly IType _element;

        public NamedType(string name, IType element)
        {
            _name = name;
            _element = element;
        }

        public override IType UpdateMu(RecursiveTypeFamily newFamily) => Dsl.Named(_name, _element.UpdateMu(newFamily));

        public override ITypeTemplate BuildTemplate() => Dsl.Named(_name, _element.Template);

        public override IOptional<IType> FindCheckedType(int index) => _element.FindCheckedType(index);

        public override IRewriteResultLeft<IPair<string, object>> All(ITypeRewriteRule rule, bool recursive, bool checkIndex)
        {
            throw new NotImplementedException();
        }

        protected override ICodec<IPair<string, object>> BuildCodec()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj, bool ignoreRecursionPoints, bool checkIndex)
        {
            throw new NotImplementedException();
        }
    }
}