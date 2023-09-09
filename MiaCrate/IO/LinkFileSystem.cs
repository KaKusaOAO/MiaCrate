namespace MiaCrate.IO;

public class LinkFileSystem : IFileSystem
{
    private bool _frozen;
    private readonly string _dummyBase = Path.GetFullPath("/");
    private readonly DummyDirectory _root = new(""); 
    
    public LinkFileSystem Freeze()
    {
        if (!_frozen) 
            _frozen = true;
        
        return this;
    }

    public LinkFileSystem Register(string path, string destination)
    {
        var splitted = Path.GetRelativePath(_dummyBase, Path.GetFullPath(path, _dummyBase))
            .Replace('\\', '/')
            .Split('/');

        var dir = _root;
        for (var i = 0; i < splitted.Length; i++)
        {
            var name = splitted[i];
            
            if (i != splitted.Length - 1)
            {
                dir = dir.GetOrCreateSubDirectory(name);
            }
            else
            {
                var file = dir.GetOrCreateFile(name);
                file.DestinationPath = destination;
            }
        }
        
        return this;
    }
    
    private DummyDirectory? ResolveDirectory(string path)
    {
        var splitted = Path.GetRelativePath(_dummyBase, Path.GetFullPath(path, _dummyBase))
            .Replace('\\', '/')
            .Split('/');
        
        var dir = _root;
        foreach (var name in splitted)
        {
            dir = dir.SubDirectories.FirstOrDefault(d => d.Name == name);
            if (dir == null) return null;
        }

        return dir;
    }

    private DummyFile? ResolveFile(string path)
    {
        var splitted = Path.GetRelativePath(_dummyBase, Path.GetFullPath(path, _dummyBase))
            .Replace('\\', '/')
            .Split('/');
        
        var dir = _root;
        for (var i = 0; i < splitted.Length; i++)
        {
            var name = splitted[i];
            
            if (i != splitted.Length - 1)
            {
                dir = dir.SubDirectories.FirstOrDefault(d => d.Name == name);
                if (dir == null) return null;
            }
            else
            {
                return dir.Files.FirstOrDefault(d => d.Name == name);
            }
        }

        return null;
    }
    
    public Stream Open(string path, FileMode mode)
    {
        var file = ResolveFile(path);
        if (file == null) throw new FileNotFoundException(path);

        return File.Open(file.DestinationPath, mode);
    }

    public Stream Open(string path, FileMode mode, FileAccess access)
    {
        var file = ResolveFile(path);
        if (file == null) throw new FileNotFoundException(path);

        return File.Open(file.DestinationPath, mode, access);
    }

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        var file = ResolveFile(path);
        if (file == null) throw new FileNotFoundException(path);

        return File.Open(file.DestinationPath, mode, access, share);
    }

    public string[] GetDirectories(string path)
    {
        var dir = ResolveDirectory(path);
        if (dir == null) throw new DirectoryNotFoundException(path);
        
        return dir.SubDirectories.Select(d => d.Name).ToArray();
    }

    public string[] GetFiles(string directory)
    {
        var dir = ResolveDirectory(directory);
        if (dir == null) throw new DirectoryNotFoundException(directory);
        
        return dir.Files.Select(d => d.Name).ToArray();
    }

    private abstract class DummyNode
    {
        public string Name { get; }
        public DummyDirectory? Parent { get; }

        protected DummyNode(string name, DummyDirectory? parent = null)
        {
            Name = name;
            Parent = parent;
        }

        public override string ToString()
        {
            var prefix = Parent == null ? "" : $"{Parent}/";
            return prefix + Name;
        }
    }
    
    private class DummyDirectory : DummyNode
    {
        private readonly Dictionary<string, DummyDirectory> _subDirs = new();
        private readonly Dictionary<string, DummyFile> _files = new();

        public List<DummyDirectory> SubDirectories => _subDirs.Values.ToList();
        public List<DummyFile> Files => _files.Values.ToList();

        public DummyDirectory(string name, DummyDirectory? parent = null) : base(name, parent)
        {
        }

        public DummyDirectory GetOrCreateSubDirectory(string name)
        {
            if (!_subDirs.ContainsKey(name))
                _subDirs[name] = new DummyDirectory(name, this);

            return _subDirs[name];
        }
        
        public DummyFile GetOrCreateFile(string name)
        {
            if (!_files.ContainsKey(name))
                _files[name] = new DummyFile(name, this);

            return _files[name];
        }
    }

    private class DummyFile : DummyNode
    {
        public string DestinationPath { get; set; }
        
        public DummyFile(string name, DummyDirectory? parent = null) : base(name, parent)
        {
        }
    }
}