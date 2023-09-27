using Mochi.Core;
using Mochi.Texts;
using Mochi.Utils;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate;

public interface IFormattedText
{
    public static IOptional<Unit> StopIteration { get; } = Optional.Of(Unit.Instance);

    public delegate IOptional<T> ContentConsumer<out T>(string str);
    public delegate IOptional<T> StyledContentConsumer<out T>(Style style, string str);
        
    public IOptional<T> Visit<T>(ContentConsumer<T> consumer);
    public IOptional<T> Visit<T>(StyledContentConsumer<T> consumer, Style style);

    public static IFormattedText FromComponent(IComponent component)
    {
        if (component.Style is not Style)
            throw new ArgumentException("The given component style is not an instance of MiaCrate Style");
        
        return new ComponentText(component);
    }

    private class ComponentText : IFormattedText
    {
        private readonly IComponent _component;

        public ComponentText(IComponent component)
        {
            _component = component;
        }

        public IOptional<T> Visit<T>(ContentConsumer<T> consumer)
        {
            var result = Optional.Empty<T>();
            
            try
            {
                _component.Visit(new ContentVisitor<T>(consumer, t => result = t), 
                    _component.Style);
            }
            catch (StopIterationException)
            {
            }
            
            return result;
        }

        public IOptional<T> Visit<T>(StyledContentConsumer<T> consumer, Style style)
        {
            var result = Optional.Empty<T>();
            
            try
            {
                _component.Visit(new StyledContentVisitor<T>(consumer, t => result = t), 
                    _component.Style);
            }
            catch (StopIterationException)
            {
            }
            
            return result;
        }

        private class ContentVisitor<T> : IContentVisitor
        {
            private readonly ContentConsumer<T> _consumer;
            private readonly Action<IOptional<T>> _callback;

            public ContentVisitor(ContentConsumer<T> consumer, Action<IOptional<T>> callback)
            {
                _consumer = consumer;
                _callback = callback;
            }

            public void Accept(IContent content, IStyle style)
            {
                if (content is not LiteralContent literal)
                {
                    content.VisitLiteral(this, style);
                    return;
                }

                var val = _consumer(literal.Text);
                _callback(val);

                if (val.IsPresent) 
                    throw new StopIterationException();
            }
        }

        private class StyledContentVisitor<T> : IContentVisitor
        {
            private readonly StyledContentConsumer<T> _consumer;
            private readonly Action<IOptional<T>> _callback;

            public StyledContentVisitor(StyledContentConsumer<T> consumer, Action<IOptional<T>> callback)
            {
                _consumer = consumer;
                _callback = callback;
            }

            public void Accept(IContent content, IStyle style)
            {
                if (content is not LiteralContent literal)
                {
                    content.VisitLiteral(this, style);
                    return;
                }

                var val = _consumer((Style) style, literal.Text);
                _callback(val);

                if (val.IsPresent) 
                    throw new StopIterationException();
            }
        }

        private class StopIterationException : Exception
        {
            
        }
    }
}