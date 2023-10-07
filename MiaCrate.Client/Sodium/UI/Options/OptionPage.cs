using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI.Options;

public class OptionPage
{
    public IComponent Name { get; }
    public List<OptionGroup> Groups { get; }
    public List<ISodiumOption> Options { get; }

    public OptionPage(IComponent name, List<OptionGroup> groups)
    {
        Name = name;
        Groups = groups;
        Options = groups.SelectMany(g => g.Options).ToList();
    }
}