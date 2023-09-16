using Mochi.Texts;

namespace MiaCrate.Client.UI.Narration;

public interface INarrationElementOutput
{
    public void Add(NarratedElementType type, INarrationThunk thunk);

    public INarrationElementOutput Nest();

    public void Add(NarratedElementType type, string content) => 
        Add(type, NarrationThunk.From(content));
    
    public void Add(NarratedElementType type, IComponent content) => 
        Add(type, NarrationThunk.From(content));
    
    public void Add(NarratedElementType type, params IComponent[] contents) => 
        Add(type, NarrationThunk.From(contents.ToList()));
}