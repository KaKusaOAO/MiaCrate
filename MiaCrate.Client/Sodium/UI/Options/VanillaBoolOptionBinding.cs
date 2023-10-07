namespace MiaCrate.Client.Sodium.UI.Options;

public class VanillaBoolOptionBinding : IOptionBinding<Client.Options, bool>
{
    private readonly OptionInstance<bool> _option;

    public VanillaBoolOptionBinding(OptionInstance<bool> option)
    {
        _option = option;
    }

    public bool GetValue(Client.Options storage) => _option.Value;

    public void SetValue(Client.Options storage, bool value) => _option.Value = value;
}