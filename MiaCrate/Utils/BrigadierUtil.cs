using MiaCrate.Texts;
using Mochi.Brigadier;
using Mochi.Brigadier.Exceptions;
using Mochi.Texts;

namespace MiaCrate;

public static class BrigadierUtil
{
    public static SimpleCommandExceptionType CreateSimpleExceptionType(IComponent component) =>
        new(new BrigadierComponent(component));
    
    public static DynamicCommandExceptionType CreateDynamicExceptionType
        (Func<object, IComponent> func) =>
        new(a => new BrigadierComponent(func(a)));

    public static Dynamic2CommandExceptionType CreateDynamic2ExceptionType
        (Func<object, object, IComponent> func) =>
        new((a, b) => new BrigadierComponent(func(a, b)));
    
    public static Dynamic3CommandExceptionType CreateDynamic3ExceptionType
        (Func<object, object, object, IComponent> func) =>
        new((a, b, c) => new BrigadierComponent(func(a, b, c)));
    
    public static Dynamic4CommandExceptionType CreateDynamic4ExceptionType
        (Func<object, object, object, object, IComponent> func) =>
        new((a, b, c, d) => new BrigadierComponent(func(a, b, c, d)));
    
    private class BrigadierComponent : IComponent, IBrigadierMessage
    {
        private readonly IComponent _inner;

        public BrigadierComponent(IComponent inner)
        {
            _inner = inner;
        }

        public IMutableComponent Clone() => _inner.Clone();

        public void Visit(IContentVisitor visitor, IStyle style) => _inner.Visit(visitor, style);

        public void VisitLiteral(IContentVisitor visitor, IStyle style) => _inner.VisitLiteral(visitor, style);

        public IContent Content => _inner.Content;

        public IStyle Style => _inner.Style;

        public IList<IComponent> Siblings => _inner.Siblings;

        public string GetString() => _inner.ToPlainText();
    }
}