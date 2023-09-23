namespace MiaCrate.Client.Graphics;

public class PanoramaRenderer
{
    private readonly CubeMap _cubeMap;
    private readonly Game _game = Game.Instance;
    private float _spin;
    private float _bob;

    public PanoramaRenderer(CubeMap cubeMap)
    {
        _cubeMap = cubeMap;
    }

    public void Render(float f, float g)
    {
        var h = f; // * panoramaSpeed
        _spin = Wrap(_spin + h * 0.1f, 360);
        _bob = Wrap(_bob + h * 0.001f, float.Pi * 2);
        _cubeMap.Render(_game, 10, -_spin, g);
    }

    private static float Wrap(float f, float g) => f > g ? f - g : f;
}