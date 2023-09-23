namespace MiaCrate.Client.Graphics;

public abstract partial class RenderStateShard
{
    public static ShaderStateShard NoShader { get; } = new();

    public static ShaderStateShard PositionColorLightmapShader { get; } =
        new(() => GameRenderer.PositionColorLightmapShader);

    public static ShaderStateShard PositionShader { get; } = new(() => GameRenderer.PositionShader);
    public static ShaderStateShard PositionColorTexShader { get; } = new(() => GameRenderer.PositionColorTexShader);
    public static ShaderStateShard PositionTexShader { get; } = new(() => GameRenderer.PositionTexShader);

    public static ShaderStateShard PositionColorTexLightmapShader { get; } =
        new(() => GameRenderer.PositionColorTexLightmapShader);

    public static ShaderStateShard PositionColorShader { get; } = new(() => GameRenderer.PositionColorShader);
    public static ShaderStateShard RenderTypeSolidShader { get; } = new(() => GameRenderer.RenderTypeSolidShader);

    public static ShaderStateShard RenderTypeCutoutMippedShader { get; } =
        new(() => GameRenderer.RenderTypeCutoutMippedShader);

    public static ShaderStateShard RenderTypeCutoutShader { get; } = new(() => GameRenderer.RenderTypeCutoutShader);

    public static ShaderStateShard RenderTypeTranslucentShader { get; } =
        new(() => GameRenderer.RenderTypeTranslucentShader);

    public static ShaderStateShard RenderTypeTranslucentMovingBlockShader { get; } =
        new(() => GameRenderer.RenderTypeTranslucentMovingBlockShader);

    public static ShaderStateShard RenderTypeTranslucentNoCrumblingShader { get; } =
        new(() => GameRenderer.RenderTypeTranslucentNoCrumblingShader);

    public static ShaderStateShard RenderTypeArmorCutoutNoCullShader { get; } =
        new(() => GameRenderer.RenderTypeArmorCutoutNoCullShader);

    public static ShaderStateShard RenderTypeEntitySolidShader { get; } =
        new(() => GameRenderer.RenderTypeEntitySolidShader);

    public static ShaderStateShard RenderTypeEntityCutoutShader { get; } =
        new(() => GameRenderer.RenderTypeEntityCutoutShader);

    public static ShaderStateShard RenderTypeEntityCutoutNoCullShader { get; } =
        new(() => GameRenderer.RenderTypeEntityCutoutNoCullShader);

    public static ShaderStateShard RenderTypeEntityCutoutNoCullZOffsetShader { get; } =
        new(() => GameRenderer.RenderTypeEntityCutoutNoCullZOffsetShader);

    public static ShaderStateShard RenderTypeItemEntityTranslucentCullShader { get; } =
        new(() => GameRenderer.RenderTypeItemEntityTranslucentCullShader);

    public static ShaderStateShard RenderTypeEntityTranslucentCullShader { get; } =
        new(() => GameRenderer.RenderTypeEntityTranslucentCullShader);
    
    public static ShaderStateShard RenderTypeEntityTranslucentShader { get; } =
        new(() => GameRenderer.RenderTypeEntityTranslucentShader);
    
    public static ShaderStateShard RenderTypeEntityTranslucentEmissiveShader { get; } =
        new(() => GameRenderer.RenderTypeEntityTranslucentEmissiveShader);
    
    public static ShaderStateShard RenderTypeEntitySmoothCutoutShader { get; } =
        new(() => GameRenderer.RenderTypeEntitySmoothCutoutShader);
    
    public static ShaderStateShard RenderTypeBeaconBeamShader { get; } =
        new(() => GameRenderer.RenderTypeBeaconBeamShader);
    
    public static ShaderStateShard RenderTypeEntityDecalShader { get; } =
        new(() => GameRenderer.RenderTypeEntityDecalShader);
    
    public static ShaderStateShard RenderTypeEntityNoOutlineShader { get; } =
        new(() => GameRenderer.RenderTypeEntityNoOutlineShader);
    
    public static ShaderStateShard RenderTypeEntityShadowShader { get; } =
        new(() => GameRenderer.RenderTypeEntityShadowShader);
    
    public static ShaderStateShard RenderTypeEntityAlphaShader { get; } =
        new(() => GameRenderer.RenderTypeEntityAlphaShader);
    
    public static ShaderStateShard RenderTypeEyesShader { get; } =
        new(() => GameRenderer.RenderTypeEyesShader);
    
    public static ShaderStateShard RenderTypeEnergySwirlShader { get; } =
        new(() => GameRenderer.RenderTypeEnergySwirlShader);

    public static ShaderStateShard RenderTypeLeashShader { get; } =
        new(() => GameRenderer.RenderTypeLeashShader);

    public static ShaderStateShard RenderTypeWaterMaskShader { get; } =
        new(() => GameRenderer.RenderTypeWaterMaskShader);

    public static ShaderStateShard RenderTypeOutlineShader { get; } =
        new(() => GameRenderer.RenderTypeOutlineShader);

    public static ShaderStateShard RenderTypeArmorGlintShader { get; } =
        new(() => GameRenderer.RenderTypeArmorGlintShader);

    public static ShaderStateShard RenderTypeArmorEntityGlintShader { get; } =
        new(() => GameRenderer.RenderTypeArmorEntityGlintShader);

    public static ShaderStateShard RenderTypeGlintTranslucentShader { get; } =
        new(() => GameRenderer.RenderTypeGlintTranslucentShader);

    public static ShaderStateShard RenderTypeGlintShader { get; } =
        new(() => GameRenderer.RenderTypeGlintShader);

    public static ShaderStateShard RenderTypeGlintDirectShader { get; } =
        new(() => GameRenderer.RenderTypeGlintDirectShader);

    public static ShaderStateShard RenderTypeEntityGlintShader { get; } =
        new(() => GameRenderer.RenderTypeEntityGlintShader);

    public static ShaderStateShard RenderTypeEntityGlintDirectShader { get; } =
        new(() => GameRenderer.RenderTypeEntityGlintDirectShader);

    public static ShaderStateShard RenderTypeCrumblingShader { get; } =
        new(() => GameRenderer.RenderTypeCrumblingShader);

    public static ShaderStateShard RenderTypeTextShader { get; } = new(() => GameRenderer.RenderTypeTextShader);
    
    public static ShaderStateShard RenderTypeTextBackgroundShader { get; } = 
        new(() => GameRenderer.RenderTypeTextBackgroundShader);
    
    public static ShaderStateShard RenderTypeTextIntensityShader { get; } = 
        new(() => GameRenderer.RenderTypeTextIntensityShader);
    
    public static ShaderStateShard RenderTypeTextSeeThroughShader { get; } = 
        new(() => GameRenderer.RenderTypeTextSeeThroughShader);
    
    public static ShaderStateShard RenderTypeTextBackgroundSeeThroughShader { get; } = 
        new(() => GameRenderer.RenderTypeTextBackgroundSeeThroughShader);
    
    public static ShaderStateShard RenderTypeTextIntensitySeeThroughShader { get; } = 
        new(() => GameRenderer.RenderTypeTextIntensitySeeThroughShader);
    
    public static ShaderStateShard RenderTypeLightningShader { get; } = 
        new(() => GameRenderer.RenderTypeLightningShader);
    
    public static ShaderStateShard RenderTypeTripwireShader { get; } = 
        new(() => GameRenderer.RenderTypeTripwireShader);
    
    public static ShaderStateShard RenderTypeEndPortalShader { get; } = 
        new(() => GameRenderer.RenderTypeEndPortalShader);
    
    public static ShaderStateShard RenderTypeEndGatewayShader { get; } = 
        new(() => GameRenderer.RenderTypeEndGatewayShader);
    
    public static ShaderStateShard RenderTypeLinesShader { get; } = 
        new(() => GameRenderer.RenderTypeLinesShader);

    public static ShaderStateShard RenderTypeGuiShader { get; } = new(() => GameRenderer.RenderTypeGuiShader);

    public static ShaderStateShard RenderTypeGuiOverlayShader { get; } =
        new(() => GameRenderer.RenderTypeGuiOverlayShader);
    
    public static ShaderStateShard RenderTypeGuiTextHighlightShader { get; } =
        new(() => GameRenderer.RenderTypeGuiTextHighlightShader);
    
    public static ShaderStateShard RenderTypeGuiGhostRecipeOverlayShader { get; } =
        new(() => GameRenderer.RenderTypeGuiGhostRecipeOverlayShader);
}