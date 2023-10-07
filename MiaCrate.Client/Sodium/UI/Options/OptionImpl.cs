using MiaCrate.Client.Sodium.UI.Options.Controls;
using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options;

public static class OptionImpl<T>
{
    public static OptionImpl<TS, T>.Builder CreateBuilder<TS>(IOptionStorage<TS> storage) => new(storage);
}

public class OptionImpl<TS, T> : ISodiumOption<T>
{
    private readonly IOptionStorage<TS> _storage;
    private readonly IOptionBinding<TS, T> _binding;

    private T _value;
    private T _modifiedValue;
    
    public IComponent Name { get; }

    public IComponent Tooltip { get; }

    public OptionImpact? Impact { get; }

    public IControl<T> Control { get; }

    public OptionFlag Flags { get; }

    public IOptionStorage Storage => _storage;

    public bool IsAvailable { get; }

    public bool HasChanged => !(_value?.Equals(_modifiedValue) ?? false);
    
    public T Value
    {
        get => _modifiedValue;
        set => _modifiedValue = value;
    }

#pragma warning disable CS8618
    private OptionImpl(IOptionStorage<TS> storage,
#pragma warning restore CS8618
        IComponent name,
        IComponent tooltip,
        IOptionBinding<TS, T> binding,
        Func<OptionImpl<TS, T>, IControl<T>> control,
        OptionFlag flags,
        OptionImpact? impact,
        bool enabled)
    {
        _storage = storage;
        Name = name;
        Tooltip = tooltip;
        _binding = binding;
        Control = control(this);
        Flags = flags;
        Impact = impact;
        IsAvailable = enabled;
        
        Reset();
    }

    public void Reset()
    {
        _value = _binding.GetValue(_storage.Data);
        _modifiedValue = _value;
    }

    public void ApplyChanges()
    {
        _binding.SetValue(_storage.Data, _modifiedValue);
        _value = _modifiedValue;
    }

    public class Builder
    {
        private readonly IOptionStorage<TS> _storage;
        private IComponent? _name;
        private IComponent? _tooltip;
        private IOptionBinding<TS, T>? _binding;
        private Func<OptionImpl<TS, T>, IControl<T>>? _control;
        private OptionImpact? _impact;
        private OptionFlag _flags;
        private bool _enabled = true;

        public Builder(IOptionStorage<TS> storage)
        {
            _storage = storage;
        }

        public Builder SetName(IComponent name)
        {
            if (name == null!)
                throw new ArgumentException("Argument must not be null");

            _name = name;
            return this;
        }
        
        public Builder SetTooltip(IComponent tooltip)
        {
            if (tooltip == null!)
                throw new ArgumentException("Argument must not be null");

            _tooltip = tooltip;
            return this;
        }

        public Builder SetBinding(Action<TS, T> setter, Func<TS, T> getter)
        {
            if (setter == null!)
                throw new ArgumentException("Setter must not be null");
            
            if (getter == null!)
                throw new ArgumentException("Getter must not be null");

            _binding = new GenericBinding<TS, T>(setter, getter);
            return this;
        }
        
        public Builder SetBinding(IOptionBinding<TS, T> binding)
        {
            if (binding == null!)
                throw new ArgumentException("Argument must not be null");

            _binding = binding;
            return this;
        }
        
        public Builder SetControl(Func<OptionImpl<TS, T>, IControl<T>> control)
        {
            if (control == null!)
                throw new ArgumentException("Argument must not be null");

            _control = control;
            return this;
        }

        public Builder SetImpact(OptionImpact? impact)
        {
            _impact = impact;
            return this;
        }

        public Builder SetEnabled(bool val)
        {
            _enabled = val;
            return this;
        }

        public Builder SetFlags(OptionFlag flags)
        {
            _flags |= flags;
            return this;
        }

        public OptionImpl<TS, T> Build()
        {
            if (_name == null!)
                throw new ArgumentException("Name must be specified");
            
            if (_tooltip == null!)
                throw new ArgumentException("Tooltip must be specified");
            
            if (_binding == null!)
                throw new ArgumentException("Option binding must be specified");
                
            if (_control == null!)
                throw new ArgumentException("Control must be specified");
            
            return new OptionImpl<TS, T>(_storage, _name, _tooltip, _binding, _control, _flags, _impact, _enabled);
        }
    }
}