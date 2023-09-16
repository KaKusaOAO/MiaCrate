using MiaCrate.Client.Platform;
using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Systems;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Shaders;

public class Program : IDisposable
{
    private const int MaxLogLength = 32678;
    private readonly ProgramType _type;
    public string Name { get; }
    public int Id { get; private set; }

    protected Program(ProgramType type, int id, string name)
    {
        Name = name;
        Id = id;
        _type = type;
    }

    public void AttachToShader(IShader shader)
    {
        RenderSystem.AssertOnRenderThread();
        GlStateManager.AttachShader(shader.Id, Id);
    }

    public void Dispose()
    {
        if (Id == -1) return;
        
        RenderSystem.AssertOnRenderThread();
        GC.SuppressFinalize(this);
        GlStateManager.DeleteShader(Id);
        Id = -1;
        _type.Programs.Remove(Name);
    }

    public static Program CompileShader(ProgramType type, string name, Stream stream, string str2,
        GlslPreprocessor preprocessor)
    {
        RenderSystem.AssertOnRenderThread();
        var shader = InternalCompileShader(type, name, stream, str2, preprocessor);
        var program = new Program(type, shader, name);
        type.Programs[name] = program;
        return program;
    }

    protected static int InternalCompileShader(ProgramType type, string name, Stream stream, string str2,
        GlslPreprocessor preprocessor)
    {
        string source;

        try
        {
            using var reader = new StreamReader(stream);
            source = reader.ReadToEnd();
        }
        catch (Exception)
        {
            throw new IOException($"Could not load program {type.Name}");
        }

        var handle = GlStateManager.CreateShader(type.Type);
        GlStateManager.ShaderSource(handle, preprocessor.Process(source));
        GlStateManager.CompileShader(handle);

        if (GlStateManager.GetShaderI(handle, ShaderParameter.CompileStatus) == 0)
        {
            var infoLog = GlStateManager.GetShaderInfoLog(handle);
            throw new IOException($"Couldn't compile {type.Name} program ({str2}, {name}) : {infoLog}");
        }

        return handle;
    }
}