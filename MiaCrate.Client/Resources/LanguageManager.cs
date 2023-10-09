using MiaCrate.Localizations;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class LanguageManager : IResourceManagerReloadListener
{
    public void OnResourceManagerReload(IResourceManager manager)
    {
        var list = new List<string>
        {
            Language.Default,
            // "zh_tw"
        };

        var lang = ClientLanguage.LoadFrom(manager, list, false);
        I18n.SetLanguage(lang);
        Language.Inject(lang);
    }
}