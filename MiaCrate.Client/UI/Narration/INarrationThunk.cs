using Mochi.Core;
using Mochi.Texts;

namespace MiaCrate.Client.UI.Narration;

public interface INarrationThunk
{
    public void GetText(Action<string> consumer);
}

// public interface INarrationThunk<T> : INarrationThunk
// {
//     
// }

public class NarrationThunk<T> : INarrationThunk // <T>
{
    private readonly T _contents;
    private readonly Action<Action<string>, T> _converter;

    internal NarrationThunk(T contents, Action<Action<string>, T> converter)
    {
        _contents = contents;
        _converter = converter;
    }

    public void GetText(Action<string> consumer) => _converter(consumer, _contents);
   
}

public static class NarrationThunk
{
    public static readonly INarrationThunk Empty = new NarrationThunk<Unit>(Unit.Instance, (_, _) => { });
    
    public static INarrationThunk From(string str)
    {
        return new NarrationThunk<string>(str, (action, s) => action(s));
    }

    public static INarrationThunk From(IComponent component)
    {
        return new NarrationThunk<IComponent>(component, (action, comp) =>
        {
            action(comp.ToPlainText());
        });
    }

    public static INarrationThunk From(List<IComponent> list)
    {
        return new NarrationThunk<List<IComponent>>(list, (action, components) =>
        {
            foreach (var s in list.Select(c => c.ToPlainText()))
            {
                action(s);
            }
        });
    }
}