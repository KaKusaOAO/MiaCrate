using Mochi.Core;
using Mochi.Texts;
using Mochi.Utils;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate;

public interface IFormattedText
{
    public static IFormattedText Empty { get; } = new EmptyText();
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

    public static IFormattedText Composite(params IFormattedText[] texts) => Composite(texts.ToList());

    public static IFormattedText Composite(List<IFormattedText> texts) => new CompositedText(texts);

    public static IFormattedText Of(string text, Style style) => new SimpleText(text, style);

    private class EmptyText : IFormattedText
    {
        public IOptional<T> Visit<T>(ContentConsumer<T> consumer) => Optional.Empty<T>();
        public IOptional<T> Visit<T>(StyledContentConsumer<T> consumer, Style style) => Optional.Empty<T>();
    }
    
    private class SimpleText : IFormattedText
    {
        private readonly string _text;
        private readonly Style _style;

        public SimpleText(string text, Style style)
        {
            _text = text;
            _style = style;
        }

        public IOptional<T> Visit<T>(ContentConsumer<T> consumer) => consumer(_text);

        public IOptional<T> Visit<T>(StyledContentConsumer<T> consumer, Style style) => consumer(_style.ApplyTo(style), _text);
    }
    
    private class CompositedText : IFormattedText
    {
        private readonly List<IFormattedText> _list;

        public CompositedText(List<IFormattedText> list)
        {
            _list = list;
        }

        public IOptional<T> Visit<T>(ContentConsumer<T> consumer)
        {
            foreach (var text in _list)
            {
                var opt = text.Visit(consumer);
                if (opt.IsPresent) return opt;
            }
            
            return Optional.Empty<T>();
        }

        public IOptional<T> Visit<T>(StyledContentConsumer<T> consumer, Style style)
        {
            foreach (var text in _list)
            {
                var opt = text.Visit(consumer, style);
                if (opt.IsPresent) return opt;
            }
            
            return Optional.Empty<T>();
        }
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