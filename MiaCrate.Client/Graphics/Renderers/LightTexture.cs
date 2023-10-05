using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;

namespace MiaCrate.Client.Graphics;

public class LightTexture : IDisposable
{
    public const int FullSky = 0xf00000;
    public const int FullBlock = 0xf0;
    public const int FullBright = FullSky | FullBlock;

    private readonly DynamicTexture _lightTexture;
    private readonly NativeImage _lightPixels;
    private readonly ResourceLocation _lightTextureLocation;
    private bool _updateLightTexture;
    private float _blockLightRedFlicker;

    private readonly GameRenderer _renderer;
    private readonly Game _game;

    public LightTexture(GameRenderer renderer, Game game)
    {
        _renderer = renderer;
        _game = game;

        _lightTexture = new DynamicTexture(16, 16, false);
        _lightTextureLocation = _game.TextureManager.RegisterDynamic("light_map", _lightTexture);
        _lightPixels = _lightTexture.Pixels!;

        for (var i = 0; i < 16; i++)
        {
            for (var j = 0; j < 16; j++)
            {
                _lightPixels.SetPixelRgba(j, i, Rgba32.White);
            }
        }
        
        _lightTexture.Upload();
    }

    public void Dispose()
    {
        _lightTexture.Dispose();
    }

    public void TurnOffLightLayer()
    {
        RenderSystem.RemoveShaderTexture(2);
    }

    public void TurnOnLightLayer()
    {
        RenderSystem.SetShaderTexture(2, _lightTextureLocation);
        _game.TextureManager.BindForSetup(_lightTextureLocation);
        _lightTexture.SetFilter(true, false);
    }

    public void Tick()
    {
        var rand = Random.Shared;
        _blockLightRedFlicker += (rand.NextSingle() - rand.NextSingle()) * rand.NextSingle() * rand.NextSingle() * 0.1f;
        _blockLightRedFlicker *= 0.9f;
        _updateLightTexture = true;
    }

    public void UpdateLightTexture(float f)
    {
        if (!_updateLightTexture) return;
        _updateLightTexture = false;

        var level = _game.Level;
        if (level == null) return;

        var g = level.GetSkyDarken(1f);
        
        Util.LogFoobar();
    }
}