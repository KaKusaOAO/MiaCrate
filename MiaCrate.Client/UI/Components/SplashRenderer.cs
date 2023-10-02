using MiaCrate.Client.Utils;

namespace MiaCrate.Client.UI;

public class SplashRenderer
{
    private const int WidthOffset = 123;
    private const int HeightOffset = 69;
    
    private readonly string _splash;

    public SplashRenderer(string splash)
    {
        _splash = splash;
    }

    public void Render(GuiGraphics graphics, int width, Font font, int alpha)
    {
        var pose = graphics.Pose;
        pose.PushPose();
        pose.Translate(width / 2f + WidthOffset, HeightOffset, 0);
        pose.MulPose(IAxis.ZP.RotationDegrees(-20));

        var f = 1.8f - MathF.Abs(MathF.Sin((Util.GetMillis() % 1000L) / 1000f * float.Pi * 2) * 0.1f);
        f = f * 100 / (font.Width(_splash) + 32);
        
        pose.Scale(f, f, f);
        graphics.DrawCenteredString(font, _splash, 0, -8, new Argb32(0xff, 0xff, 0x00, (byte) alpha));
        pose.PopPose();
    }
}