using System.Diagnostics;
using System.IO.Compression;
using MiaCrate.Extensions;
using MiaCrate.IO;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class VanillaPackResourcesBuilder
{
    private BuiltInMetadata _metadata;
    private readonly HashSet<string> _namespaces = new();
    private static readonly Dictionary<PackType, FileSystemInfo> _rootDirByType = Util.Make(() =>
    {
        var dict = new Dictionary<PackType, FileSystemInfo>();
        var stream = ResourceAssembly.GetResource("1.20.2-pre2.jar");

        if (stream == null)
        {
            Logger.Warn("Cannot read JAR file");
            return dict;
        }
        
        var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        foreach (var type in PackType.Values)
        {
            var assetRootPath = $"{type.Directory}/.mcassetsroot";
            if (archive.GetEntry(assetRootPath) == null)
            {
                Logger.Warn($"File {assetRootPath} does not exist in JAR file");
            }
            else
            {
                dict.Add(type, new FileSystemInfo(
                        new ZipFileSystem(archive),
                        new ZipFileSystem(archive, $"/{type.Directory}")
                    )
                );
            }
        }
        
        return dict;
    });

    private readonly HashSet<IFileSystem> _rootPaths = new();
    private readonly Dictionary<PackType, HashSet<IFileSystem>> _pathsForType = new();

    public VanillaPackResourcesBuilder SetMetadata(BuiltInMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    public VanillaPackResourcesBuilder ExposeNamespace(params string[] namespaces)
    {
        foreach (var ns in namespaces)
        {
            _namespaces.Add(ns);
        }

        return this;
    }

    public VanillaPackResourcesBuilder ApplyDevelopmentConfig()
    {
        return this;
    }

    public VanillaPackResourcesBuilder PushJarResources()
    {
        foreach (var (key, (root, value)) in _rootDirByType)
        {
            PushRootPath(root);
            PushPathForType(key, value);
        }

        return this;
    }

    private void PushRootPath(IFileSystem fs)
    {
        _rootPaths.Add(fs);
    }

    private void PushPathForType(PackType type, IFileSystem fs)
    {
        _pathsForType
            .ComputeIfAbsent(type, _ => new HashSet<IFileSystem>())
            .Add(fs);
    }

    public VanillaPackResourcesBuilder PushAssetPath(PackType type, IFileSystem fs)
    {
        PushRootPath(fs);
        PushPathForType(type, fs);
        return this;
    }

    public VanillaPackResources Build()
    {
        return new VanillaPackResources(_metadata, _namespaces, 
            _rootPaths.ToList(),
            _pathsForType.ToDictionary(
                x => x.Key,
                x => x.Value.ToList()
            )    
        );
    }

    private class FileSystemInfo
    {
        public FileSystemInfo(IFileSystem root, IFileSystem byType)
        {
            Root = root;
            ByType = byType;
        }

        public IFileSystem Root { get; }
        public IFileSystem ByType { get; }

        public void Deconstruct(out IFileSystem root, out IFileSystem byType)
        {
            root = Root;
            byType = ByType;
        }
    }
}