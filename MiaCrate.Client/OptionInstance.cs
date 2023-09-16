using MiaCrate.Client.UI;
using MiaCrate.Data;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Client;

public interface IOptionInstance
{
    
}

public interface IOptionInstance<T> : IOptionInstance
{
    
}

public sealed class OptionInstance<T> : IOptionInstance<T>
{
    private readonly TooltipSupplier _tooltip;
    public IValueSet Values { get; }
    public ICodec<T> Codec { get; }
    private readonly Func<T, IComponent> _toString;
    private readonly T _initialValue;
    private readonly Action<T> _onValueUpdate;
    private IComponent _caption;
    private T _value;

    public T Value
    {
        get => _value;
        set => InternalSet(value);
    }

    public OptionInstance(string captionKey, TooltipSupplier tooltip, CaptionBasedToString captionBasedToString,
        IValueSet values, T obj, Action<T> consumer)
        : this(captionKey, tooltip, captionBasedToString, values, values.Codec, obj, consumer)
    {
        
    }

    public OptionInstance(string captionKey, TooltipSupplier tooltip, CaptionBasedToString captionBasedToString,
        IValueSet values, ICodec<T> codec, T initialValue, Action<T> onValueUpdate)
    {
        _caption = TranslateText.Of(captionKey);
        _tooltip = tooltip;
        _toString = v => captionBasedToString(_caption, v);
        Values = values;
        Codec = codec;
        _initialValue = initialValue;
        _onValueUpdate = onValueUpdate;
        _value = _initialValue;
    }
    
    private void InternalSet(T value)
    {
        var result = Values.ValidateValue(value).OrElseGet(() =>
        {
            Logger.Error($"Illegal option value {value} for {_caption}");
            return _initialValue;
        });

        if (!Game.Instance.IsRunning)
        {
            _value = result;
        }
        else
        {
            if (Equals(_value, result)) return;
            
            _value = result;
            _onValueUpdate(_value);
        }
    }

    public AbstractWidget CreateButton(Options options, int x, int y, int width, Action<T> consumer) => 
        Values.CreateButton(_tooltip, options, x, y, width, consumer)(this);

    public override string ToString() => _caption.ToPlainText();

    public delegate Tooltip? TooltipSupplier(T obj);

    public delegate IComponent CaptionBasedToString(IComponent component, T obj);

    public record EnumValue(List<T> Values, ICodec<T> Codec) : ICycleableValueSet
    {
        public IOptional<T> ValidateValue(T obj) => Values.Contains(obj) ? Optional.Of(obj) : Optional.Empty<T>();
    }
    
    public interface IValueSet
    {
        public Func<OptionInstance<T>, AbstractWidget> CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> consumer);
        public IOptional<T> ValidateValue(T obj);
        public ICodec<T> Codec { get; }
    }
    
    public interface ICycleableValueSet : IValueSet
    {
        public new Func<OptionInstance<T>, AbstractWidget> CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> onValueChanged)
        {
            return instance => CycleButton<T>.Builder(instance._toString)
                .Create(x, y, width, Button.DefaultHeight, instance._caption, (b, obj) =>
                {
                    options.Save();
                    onValueChanged(obj);
                });
        }

        Func<OptionInstance<T>, AbstractWidget> IValueSet.CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> onValueChanged) =>
            CreateButton(tooltipSupplier, options, x, y, width, onValueChanged);
    }
}

public static class OptionInstance
{
    // public static OptionInstance<bool> CreateBool()
}