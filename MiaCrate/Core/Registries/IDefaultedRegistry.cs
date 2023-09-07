namespace MiaCrate.Core;

public interface IDefaultedRegistry : IRegistry
{
    public ResourceLocation DefaultKey { get; }
}