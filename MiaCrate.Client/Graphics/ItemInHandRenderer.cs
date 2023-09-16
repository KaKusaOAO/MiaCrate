namespace MiaCrate.Client.Graphics;

public class ItemInHandRenderer
{
    private const float ItemSwingXPosScale = -0.4F;
    private const float ItemSwingYPosScale = 0.2F;
    private const float ItemSwingZPosScale = -0.2F;
    private const float ItemHeightScale = -0.6F;
    private const float ItemPosX = 0.56F;
    private const float ItemPosY = -0.52F;
    private const float ItemPosZ = -0.72F;
    private const float ItemPreSwingRotY = 45.0F;
    private const float ItemSwingXRotAmount = -80.0F;
    private const float ItemSwingYRotAmount = -20.0F;
    private const float ItemSwingZRotAmount = -20.0F;
    private const float EatJiggleXRotAmount = 10.0F;
    private const float EatJiggleYRotAmount = 90.0F;
    private const float EatJiggleZRotAmount = 30.0F;
    private const float EatJiggleXPosScale = 0.6F;
    private const float EatJiggleYPosScale = -0.5F;
    private const float EatJiggleZPosScale = 0.0F;
    private const double EatJiggleExponent = 27.0;
    private const float EatExtraJiggleCutoff = 0.8F;
    private const float EatExtraJiggleScale = 0.1F;
    private const float ArmSwingXPosScale = -0.3F;
    private const float ArmSwingYPosScale = 0.4F;
    private const float ArmSwingZPosScale = -0.4F;
    private const float ArmSwingYRotAmount = 70.0F;
    private const float ArmSwingZRotAmount = -20.0F;
    private const float ArmHeightScale = -0.6F;
    private const float ArmPosScale = 0.8F;
    private const float ArmPosX = 0.8F;
    private const float ArmPosY = -0.75F;
    private const float ArmPosZ = -0.9F;
    private const float ArmPreSwingRotY = 45.0F;
    private const float ArmPreRotationXOffset = -1.0F;
    private const float ArmPreRotationYOffset = 3.6F;
    private const float ArmPreRotationZOffset = 3.5F;
    private const float ArmPostRotationXOffset = 5.6F;
    private const int ArmRotX = 200;
    private const int ArmRotY = -135;
    private const int ArmRotZ = 120;
    private const float MapSwingXPosScale = -0.4F;
    private const float MapSwingZPosScale = -0.2F;
    private const float MapHandsPosX = 0.0F;
    private const float MapHandsPosY = 0.04F;
    private const float MapHandsPosZ = -0.72F;
    private const float MapHandsHeightScale = -1.2F;
    private const float MapHandsTiltScale = -0.5F;
    private const float MapPlayerPitchScale = 45.0F;
    private const float MapHandsZRotAmount = -85.0F;
    private const float MapHandXRotAmount = 45.0F;
    private const float MapHandYRotAmount = 92.0F;
    private const float MapHandZRotAmount = -41.0F;
    private const float MapHandXPos = 0.3F;
    private const float MapHandYPos = -1.1F;
    private const float MapHandZPos = 0.45F;
    private const float MapSwingXRotAmount = 20.0F;
    private const float MapPreRotScale = 0.38F;
    private const float MapGlobalXPos = -0.5F;
    private const float MapGlobalYPos = -0.5F;
    private const float MapGlobalZPos = 0.0F;
    private const float MapFinalScale = 0.0078125F;
    private const int MapBorder = 7;
    private const int MapHeight = 128;
    private const int MapWidth = 128;
    private const float BowChargeXPosScale = 0.0F;
    private const float BowChargeYPosScale = 0.0F;
    private const float BowChargeZPosScale = 0.04F;
    private const float BowChargeShakeXScale = 0.0F;
    private const float BowChargeShakeYScale = 0.004F;
    private const float BowChargeShakeZScale = 0.0F;
    private const float BowChargeZScale = 0.2F;
    private const float BowMinShakeCharge = 0.1F;
    
    public ItemInHandRenderer(Game game, EntityRenderDispatcher entityRenderDispatcher, ItemRenderer itemRenderer)
    {
        
    }

    private float CalculateMapTilt(float f)
    {
        var g = 1.0F - f / MapPlayerPitchScale + 0.1F;
        g = Math.Clamp(g, 0.0F, 1.0F);
        g = MathF.Cos(g * MathF.PI) * MapHandsTiltScale + 0.5F;
        return g;
    }

    private void RenderTwoHandedMap(PoseStack stack, IMultiBufferSource bufferSource, int i, float f, float g, float h)
    {
        var j = MathF.Sqrt(h);
        var k = MapSwingZPosScale * MathF.Sin(h * MathF.PI);
        var l = MapSwingXPosScale * MathF.Sin(j * MathF.PI);
        throw new NotImplementedException();
    }
}