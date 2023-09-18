using System.Text.Json.Nodes;
using MiaCrate.Client.Colors;
using MiaCrate.Client.Models;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class ModelBakery
{
    public const int DestroyStageCount = 10;
    private const int InvisibleModelGroup = 0;
    private const string BuiltinSlash = "builtin/";
    private const string BuiltinSlashGenerated = $"{BuiltinSlash}generated";
    private const string BuiltinBlockEntity = $"{BuiltinSlash}entity";
    private const string MissingModelName = "missing";

    public static readonly ModelResourceLocation MissingModelLocation = ModelResourceLocation.Vanilla($"{BuiltinSlash}missing", "missing");
    public static readonly FileToIdConverter BlockStateLister = FileToIdConverter.Json("blockstates");
    public static readonly FileToIdConverter ModelLister = FileToIdConverter.Json("models");

    public ModelBakery(BlockColors blockColors, IProfilerFiller profiler,
        Dictionary<ResourceLocation, BlockModel> modelResources,
        Dictionary<ResourceLocation, List<LoadedJson>> blockStateResources)
    {
        
    }

    public record LoadedJson(string Source, JsonNode Data);
}