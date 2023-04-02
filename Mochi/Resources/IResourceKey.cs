namespace Mochi.Resources;

public interface IResourceKey
{
    public ResourceLocation Registry { get; }
    public ResourceLocation Location { get; }
}

public interface IResourceKey<out T> : IResourceKey
{
    
}