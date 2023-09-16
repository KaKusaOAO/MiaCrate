using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;

namespace MiaCrate.Client.Graphics;

public class EffectProgram : Program
{
    private static readonly GlslPreprocessor Preprocessor = new PreprocessorInstance();
    
    private int _references;
    
    private EffectProgram(ProgramType type, int id, string name) : base(type, id, name)
    {
    }

    public void AttachToEffect(IEffect effect)
    {
        RenderSystem.AssertOnRenderThread();
        ++_references;
        AttachToShader(effect);
    }

    public static EffectProgram CompileShader(ProgramType type, string name, Stream stream, string str2)
    {
        RenderSystem.AssertOnRenderThread();
        var shader = InternalCompileShader(type, name, stream, str2, Preprocessor);
        var program = new EffectProgram(type, shader, name);
        type.Programs[name] = program;
        return program;
    }

    private class PreprocessorInstance : GlslPreprocessor
    {
        public override string ApplyImport(bool flag, string path) => "#error Import statement not supported";
    }
}