using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Common;
using MiaCrate.Core;
using Mochi.Utils;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class CubeMap
{
    private const int Sides = 6;
    
    private readonly ResourceLocation[] _images = new ResourceLocation[Sides];

    public CubeMap(ResourceLocation location)
    {
        for (var i = 0; i < Sides; i++)
        {
            _images[i] = location.WithSuffix($"_{i}.png");
        }
    }

    public void Render(Game game, float f, float g, float alpha)
    {
        var tesselator = Tesselator.Instance;
        var builder = tesselator.Builder;
        var matrix =
            Matrix4.CreatePerspectiveFieldOfView(1.4835298f, (float) game.Window.Width / game.Window.Height, 0.05f, 10f);
        RenderSystem.BackupProjectionMatrix();
        
        RenderSystem.SetProjectionMatrix(matrix, IVertexSorting.DistanceToOrigin);
        var poseStack = RenderSystem.ModelViewStack;
        poseStack.PushPose();
        poseStack.SetIdentity();
        poseStack.MulPose(IAxis.XP.RotationDegrees(180f));
        
        RenderSystem.ApplyModelViewMatrix();
        RenderSystem.SetShader(() => GameRenderer.PositionTexColorShader);
        RenderSystem.EnableBlend();
        RenderSystem.DisableCull();
        RenderSystem.DepthMask(false);

        for (var j = 0; j < 4; j++)
        {
            poseStack.PushPose();
            var k = (j % 2 / 2f - 0.5f) / 256f;
            var l = (j / 2f / 2f - 0.5f) / 256f;
            var m = 0f;
            
            poseStack.Translate(k, l, m);
            poseStack.MulPose(IAxis.XP.RotationDegrees(f));
            poseStack.MulPose(IAxis.YP.RotationDegrees(g));
            RenderSystem.ApplyModelViewMatrix();

            for (var n = 0; n < Sides; n++)
            {
                RenderSystem.SetShaderTexture(0, _images[n]);
                builder.Begin(VertexFormat.Mode.Quads, DefaultVertexFormat.PositionTexColor);
                var o = (int) (Math.Round(255 * alpha) / (j + 1));

                if (n == 0)
                {
                    builder.Vertex(-1.0, -1.0, 1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
                    builder.Vertex(-1.0, 1.0, 1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
                    builder.Vertex(1.0, 1.0, 1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
                    builder.Vertex(1.0, -1.0, 1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
                }
                
                if (n == 1) 
                {
					builder.Vertex(1.0, -1.0, 1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, 1.0, 1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, 1.0, -1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, -1.0, -1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
				}

				if (n == 2) 
				{
					builder.Vertex(1.0, -1.0, -1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, 1.0, -1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, 1.0, -1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, -1.0, -1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
				}

				if (n == 3) 
				{
					builder.Vertex(-1.0, -1.0, -1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, 1.0, -1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, 1.0, 1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, -1.0, 1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
				}

				if (n == 4) 
				{
					builder.Vertex(-1.0, -1.0, -1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, -1.0, 1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, -1.0, 1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, -1.0, -1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
				}

				if (n == 5) 
				{
					builder.Vertex(-1.0, 1.0, 1.0).Uv(0.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(-1.0, 1.0, -1.0).Uv(0.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, 1.0, -1.0).Uv(1.0F, 1.0F).Color(255, 255, 255, o).EndVertex();
					builder.Vertex(1.0, 1.0, 1.0).Uv(1.0F, 0.0F).Color(255, 255, 255, o).EndVertex();
				}
                
                tesselator.End();
            }
            
            poseStack.PopPose();
            RenderSystem.ApplyModelViewMatrix();
            RenderSystem.ColorMask(true, true, true, false);
        }
        
        RenderSystem.ColorMask(true, true, true, true);
        RenderSystem.RestoreProjectionMatrix();
        
        poseStack.PopPose();
        RenderSystem.ApplyModelViewMatrix();
        
        RenderSystem.DepthMask(true);
        RenderSystem.EnableCull();
        RenderSystem.EnableDepthTest();
    } 
    
    public Task PreloadAsync(TextureManager manager, IExecutor executor)
    {
        return Task.WhenAll(
            _images.Select(x => manager.PreloadAsync(x, executor))
        );
    }
}