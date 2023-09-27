using System.Collections.Immutable;

namespace MiaCrate.Commands;

public interface ISharedSuggestionProvider
{
    public IEnumerable<string> OnlinePlayerNames { get; }

    public IEnumerable<string> CustomTabSuggestions => OnlinePlayerNames;
    
    public IEnumerable<string> SelectedEntities => ImmutableList<string>.Empty;

}