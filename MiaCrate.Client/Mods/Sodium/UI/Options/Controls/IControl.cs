namespace MiaCrate.Client.Sodium.UI.Options.Controls;

public interface IControl
{
    public int MaxWidth { get; }
    
    public IControlElement CreateElement(Dim2I dim);
}

public interface IControl<T> : IControl
{
    public ISodiumOption<T> Option { get; }

    public new ControlElement<T> CreateElement(Dim2I dim);
    IControlElement IControl.CreateElement(Dim2I dim) => CreateElement(dim);
}