using MiaCrate.Client.Resources;

namespace MiaCrate.Client.Graphics;

public class ItemModelShaper
{
    public ModelManager ModelManager { get; }

    public ItemModelShaper(ModelManager modelManager)
    {
        ModelManager = modelManager;
    }

    public void RebuildCache()
    {
        
    }
}