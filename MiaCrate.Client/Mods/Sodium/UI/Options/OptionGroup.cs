namespace MiaCrate.Client.Sodium.UI.Options;

public class OptionGroup
{
    public List<ISodiumOption> Options { get; }

    public OptionGroup(List<ISodiumOption> options)
    {
        Options = options;
    }

    public static Builder CreateBuilder() => new();

    public class Builder
    {
        private readonly List<ISodiumOption> _options = new();

        public Builder Add(ISodiumOption option)
        {
            _options.Add(option);
            return this;
        }

        public OptionGroup Build()
        {
            if (!_options.Any())
                throw new InvalidOperationException("At least one option must be specified");

            return new OptionGroup(_options);
        }
    }
}