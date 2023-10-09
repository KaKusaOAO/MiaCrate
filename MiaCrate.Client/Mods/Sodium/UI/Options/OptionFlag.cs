namespace MiaCrate.Client.Sodium.UI.Options;

[Flags]
public enum OptionFlag
{
    None,
    RequiresRendererReload,
    RequiresRendererUpdate = 1 << 1,
    RequiresAssetReload = 1 << 2,
    RequiresGameRestart = 1 << 3
}