namespace MiaCrate.Client.Resources;

public class ModelResourceLocation : ResourceLocation
{
    public const char VariantSeparator = '#';
    public string Variant { get; }

    public ModelResourceLocation(ResourceLocation location, string variant)
        : this(location.Namespace, location.Path, LowerCaseVariant(variant), null) {}

    public ModelResourceLocation(string ns, string path, string variant) : base(ns, path)
    {
        Variant = LowerCaseVariant(variant);
    }

    protected ModelResourceLocation(string ns, string path, string variant, Dummy? dummy) : base(ns, path, dummy)
    {
        Variant = variant;
    }

    public static ModelResourceLocation Vanilla(string path, string variant)
        => new(DefaultNamespace, path, variant);

    private static string LowerCaseVariant(string variant) => variant.ToLowerInvariant();

    public override string ToString() => $"{base.ToString()}{VariantSeparator}{Variant}";
}