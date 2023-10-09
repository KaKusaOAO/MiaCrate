using MiaCrate.Client.UI;
using MiaCrate.Data;
using MiaCrate.Texts;
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
        _caption = MiaComponent.Translatable(captionKey);
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

    public AbstractWidget CreateButton(Options options, int x, int y, int width) =>
        CreateButton(options, x, y, width, _ => { });
    
    public AbstractWidget CreateButton(Options options, int x, int y, int width, Action<T> consumer) => 
        Values.CreateButton(_tooltip, options, x, y, width, consumer)(this);

    public override string ToString() => _caption.ToPlainText();

    public delegate Tooltip? TooltipSupplier(T obj);

    public delegate IComponent CaptionBasedToString(IComponent component, T obj);

    public record EnumValue(List<T> Values, ICodec<T> Codec) : ICycleableValueSet
    {
        public IOptional<T> ValidateValue(T val) => Values.Contains(val) ? Optional.Of(val) : Optional.Empty<T>();
    }

    public interface IValueSet
    {
        public Func<OptionInstance<T>, AbstractWidget> CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> consumer);
        public IOptional<T> ValidateValue(T val);
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
    
    public interface ISliderableValueSet : IValueSet
    {
        public double ToSliderValue(T val);

        public T FromSliderValue(double sliderValue);
        
        public new Func<OptionInstance<T>, AbstractWidget> CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> onValueChanged) =>
            instance => new OptionInstanceSliderButton(options, x, y, width, Button.DefaultHeight, instance,
                this, tooltipSupplier, onValueChanged);

        Func<OptionInstance<T>, AbstractWidget> IValueSet.CreateButton(TooltipSupplier tooltipSupplier, Options options,
            int x, int y, int width, Action<T> onValueChanged) =>
            CreateButton(tooltipSupplier, options, x, y, width, onValueChanged);
    }

    private sealed class OptionInstanceSliderButton : AbstractOptionSliderButton
    {
        private readonly OptionInstance<T> _instance;
        private readonly ISliderableValueSet _values;
        private readonly TooltipSupplier _tooltipSupplier;
        private readonly Action<T> _onValueChanged;

        public OptionInstanceSliderButton(Options options, int x, int y, int width, int height, OptionInstance<T> instance, ISliderableValueSet values, TooltipSupplier tooltipSupplier, Action<T> onValueChanged) 
            : base(options, x, y, width, height, values.ToSliderValue(instance.Value))
        {
            _instance = instance;
            _values = values;
            _tooltipSupplier = tooltipSupplier;
            _onValueChanged = onValueChanged;
            UpdateMessage();
        }

        protected override void ApplyValue()
        {
            _instance.Value = _values.FromSliderValue(Value);
            Options.Save();
            _onValueChanged(_instance.Value);
        }

        protected override void UpdateMessage()
        {
            Message = _instance._toString(_instance.Value);
            Tooltip = _tooltipSupplier(_values.FromSliderValue(Value));
        }
    }
}

public static class OptionInstance
{
    private static readonly OptionInstance<bool>.CaptionBasedToString _boolToStr = (_, b) =>
        b ? CommonComponents.OptionOn : CommonComponents.OptionOff;
    
    private static readonly OptionInstance<bool>.EnumValue _boolValues = new(new List<bool> {true, false}, Codec.Bool);

    public static OptionInstance<T>.TooltipSupplier NoTooltip<T>() => _ => null;

    public static OptionInstance<bool> CreateBool(string name, bool bl, Action<bool> onChanged) =>
        CreateBool(name, NoTooltip<bool>(), bl, onChanged);

    public static OptionInstance<bool> CreateBool(string name, bool bl) =>
        CreateBool(name, NoTooltip<bool>(), bl, _ => { });
    
    public static OptionInstance<bool> CreateBool(string name, OptionInstance<bool>.TooltipSupplier tooltipSupplier,
        bool bl) =>
        CreateBool(name, tooltipSupplier, bl, _ => { }); 
    
    public static OptionInstance<bool> CreateBool(string name, OptionInstance<bool>.TooltipSupplier tooltipSupplier,
        bool bl, Action<bool> onChanged) =>
        CreateBool(name, tooltipSupplier, _boolToStr, bl, onChanged);
    
    public static OptionInstance<bool> CreateBool(string name, OptionInstance<bool>.TooltipSupplier tooltipSupplier,
        OptionInstance<bool>.CaptionBasedToString captionBasedToString, bool bl, Action<bool> onChanged)
    {
        return new OptionInstance<bool>(name, tooltipSupplier, captionBasedToString, _boolValues, bl, onChanged);
    }

    public record IntRange(int MinInclusive, int MaxInclusive) : IIntRangeBase
    {
        public IOptional<int> ValidateValue(int val)
        {
            return val.CompareTo(MinInclusive) >= 0 && val.CompareTo(MaxInclusive) <= 0
                ? Optional.Of(val)
                : Optional.Empty<int>();
        }

        public ICodec<int> Codec => Data.Codec.IntRange(MinInclusive, MaxInclusive + 1);
    }
    
    public interface IIntRangeBase : OptionInstance<int>.ISliderableValueSet
    {
        public int MinInclusive { get; }
        public int MaxInclusive { get; }

        double OptionInstance<int>.ISliderableValueSet.ToSliderValue(int val) => 
            Util.Map(val, MinInclusive, MaxInclusive, 0, 1);

        int OptionInstance<int>.ISliderableValueSet.FromSliderValue(double sliderValue) => 
            (int) Math.Floor(Util.Map(sliderValue, 0, 1, MinInclusive, MaxInclusive));
    }

    public class UnitDouble : OptionInstance<double>.ISliderableValueSet
    {
        public static UnitDouble Instance { get; } = new();
        
        private UnitDouble() {}

        public ICodec<double> Codec => Data.Codec.Double;
        
        public IOptional<double> ValidateValue(double val)
        {
            return val is >= 0 and <= 1 
                ? Optional.Of(val) 
                : Optional.Empty<double>();
        }

        public double ToSliderValue(double val) => val;

        public double FromSliderValue(double sliderValue) => sliderValue;
    }
}