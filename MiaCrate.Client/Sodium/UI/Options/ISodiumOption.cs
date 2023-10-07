using MiaCrate.Client.Sodium.UI.Options.Controls;
using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options;

public interface ISodiumOption
{
    public IComponent Name { get; }
    public IComponent Tooltip { get; }
    public IControl Control { get; }
    public OptionImpact? Impact { get; }
    public OptionFlag Flags { get; }
    public IOptionStorage Storage { get; }
    
    public bool IsAvailable { get; }
    public bool HasChanged { get; }
    
    public void Reset();
    public void ApplyChanges();
}

public interface ISodiumOption<T> : ISodiumOption
{
    public new IControl<T> Control { get; }
    IControl ISodiumOption.Control => Control;
    
    public T Value { get; set; }
}