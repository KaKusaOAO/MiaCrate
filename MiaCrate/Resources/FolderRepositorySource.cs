using Mochi.Utils;

namespace MiaCrate.Resources;

public class FolderRepositorySource : IRepositorySource
{
    private readonly string _folder;
    private readonly PackType _packType;
    private readonly IPackSource _packSource;

    public FolderRepositorySource(string folder, PackType packType, IPackSource packSource)
    {
        _folder = folder;
        _packType = packType;
        _packSource = packSource;
    }


    public void LoadPacks(Action<Pack> loader)
    {
        try
        {
            Directory.CreateDirectory(_folder);
        }
        catch (IOException ex)
        {
            Logger.Warn($"Failed to list packs in {_folder}");
            Logger.Warn(ex);
        }
    }
}