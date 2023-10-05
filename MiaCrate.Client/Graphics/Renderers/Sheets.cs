namespace MiaCrate.Client.Graphics;

public static class Sheets
{
    public static ResourceLocation ShulkerSheet { get; } = new("textures/atlas/shulker_boxes.png");
    public static ResourceLocation BedSheet { get; } = new("textures/atlas/beds.png");
    public static ResourceLocation BannerSheet { get; } = new("textures/atlas/banner_patterns.png");
    public static ResourceLocation ShieldSheet { get; } = new("textures/atlas/shield_patterns.png");
    public static ResourceLocation SignSheet { get; } = new("textures/atlas/signs.png");
    public static ResourceLocation ChestSheet { get; } = new("textures/atlas/chest.png");
    public static ResourceLocation ArmorTrimsSheet { get; } = new("textures/atlas/armor_trims.png");
    public static ResourceLocation DecoratedPotSheet { get; } = new("textures/atlas/decorated_pot.png");

    public static RenderType ShulkerBoxSheetType { get; } = RenderType.EntityCutoutNoCull(ShulkerSheet);
}