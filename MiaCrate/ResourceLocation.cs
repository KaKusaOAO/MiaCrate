using System.Diagnostics.CodeAnalysis;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate;

public class ResourceLocation : IComparable<ResourceLocation>
{
    public static ICodec<ResourceLocation> Codec { get; } = Data.Codec.String
        .CoSelectSelectMany(Read, x => x.ToString())
        .Stable;

    public const char NamespaceSeparator = ':';
    public const string DefaultNamespace = "minecraft";
    public const string RealmsNamespace = "realms";
    
    public string Path { get; }
    public string Namespace { get; }

    protected static string[] Decompose(string str, char delimiter)
    {
        var arr = new[] {DefaultNamespace, str};
        var i = str.IndexOf(delimiter);

        if (i >= 0)
        {
            arr[1] = str[(i + 1)..];
            
            if (i >= 1)
            {
                arr[0] = str[..i];
            }
        }

        return arr;
    }
    
    public ResourceLocation(string path) : this(Decompose(path, NamespaceSeparator)) { }
    
    public ResourceLocation(string[] arr) : this(arr[0], arr[1]) { }
    
    public ResourceLocation(string ns, string path) 
        : this(AssertValidNamespace(ns, path), AssertValidPath(ns, path), null) {}

    protected ResourceLocation(string ns, string path, Dummy? dummy)
    {
        Namespace = ns;
        Path = path;
    }
    
    public static bool IsValidPathChar(char c) => 
        c is '_' or '-' or >= 'a' and <= 'z' or >= '0' and <= '9' or '/' or '.';

    private static bool IsValidNamespaceChar(char c) => 
        c is '_' or '-' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.';

    private static bool IsValidNamespace(string str) => str.All(IsValidNamespaceChar);

    private static bool IsValidPath(string str) => str.All(IsValidPathChar);

    private static string AssertValidNamespace(string ns, string path)
    {
        if (!IsValidNamespace(ns))
            throw new ResourceLocationException($"Non [a-z0-9_.-] character in namespace of location: " +
                                                $"{ns}{NamespaceSeparator}{path}");

        return ns;
    }

    private static string AssertValidPath(string ns, string path)
    {
        if (!IsValidPath(path))
            throw new ResourceLocationException($"Non [a-z0-9/._-] character in path of location:  " +
                                                $"{ns}{NamespaceSeparator}{path}");

        return path;
    }
    
    public static implicit operator string(ResourceLocation location) => location.ToString();

    public static implicit operator ResourceLocation(string raw)
    {
        if (!raw.Contains(NamespaceSeparator)) return new ResourceLocation(raw);
        
        var split = raw.Split(NamespaceSeparator, 2);
        return new ResourceLocation(split[0], split[1]);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ResourceLocation other) return false;
        return Namespace == other.Namespace && Path == other.Path;
    }

    public static bool operator ==(ResourceLocation? a, ResourceLocation? b) => a?.Equals(b) ?? false;
    public static bool operator !=(ResourceLocation? a, ResourceLocation? b) => !(a == b);

    public override int GetHashCode() => HashCode.Combine(Namespace, Path);

    public override string ToString() => $"{Namespace}{NamespaceSeparator}{Path}";

    public static IDataResult<ResourceLocation> Read(string str)
    {
        try
        {
            return DataResult.Success(new ResourceLocation(str));
        }
        catch (ResourceLocationException ex)
        {
            return DataResult.Error<ResourceLocation>(() => 
                $"Not a valid resource location: {str} {ex.Message}"
            );
        }
    }

    public ResourceLocation WithPath(string path) => 
        new(Namespace, AssertValidPath(Namespace, path), null);
    
    public ResourceLocation WithPrefix(string prefix) =>
        WithPath(prefix + Path);
    
    public ResourceLocation WithSuffix(string suffix) =>
        WithPath(Path + suffix);

    public static bool TryParse(string str, [MaybeNullWhen(false)] out ResourceLocation result)
    {
        try
        {
            result = new ResourceLocation(str);
            return true;
        }
        catch (ResourceLocationException)
        {
            result = null;
            return false;
        }
    }
    
    public static bool TryBuild(string ns, string path, [MaybeNullWhen(false)] out ResourceLocation result)
    {
        try
        {
            result = new ResourceLocation(ns, path);
            return true;
        }
        catch (ResourceLocationException)
        {
            result = null;
            return false;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    protected record Dummy;

    public int CompareTo(ResourceLocation? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var pathComparison = string.Compare(Path, other.Path, StringComparison.Ordinal);
        if (pathComparison != 0) return pathComparison;
        return string.Compare(Namespace, other.Namespace, StringComparison.Ordinal);
    }
}