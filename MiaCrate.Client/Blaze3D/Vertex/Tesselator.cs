using MiaCrate.Client.Systems;

namespace MiaCrate.Client;

public class Tesselator
{
    private const int MaxFloats = 0x200000;

    private static readonly Tesselator _instance = new();

    public static Tesselator Instance
    {
        get
        {
            RenderSystem.AssertOnGameThreadOrInit();
            return _instance;
        }
    }
    
    public BufferBuilder Builder { get; set; }
    
    public Tesselator(int size = MaxFloats)
    {
        Builder = new BufferBuilder(size);
    }

    public void End()
    {
        BufferUploader.DrawWithShader(Builder.End());
    }
}