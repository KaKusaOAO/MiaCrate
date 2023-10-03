using MiaCrate.Data;

namespace MiaCrate.DataFixes;

public static class References
{
    public static Dsl.ITypeReference Level { get; } = Dsl.CreateReference(() => "level");
    public static Dsl.ITypeReference BlockEntity { get; } = Dsl.CreateReference(() => "block_entity");
}