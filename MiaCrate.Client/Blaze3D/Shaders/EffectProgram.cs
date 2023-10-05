using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Systems;
using Veldrid;

namespace MiaCrate.Client.Shaders;

public class EffectProgram : Program
{
    private static readonly GlslPreprocessor Preprocessor = new PreprocessorInstance();
    
    private int _references;
    
    public EffectProgram(ProgramType type, Shader id, string name) : base(type, id, name)
    {
    }

    public void AttachToEffect(IEffect effect)
    {
        RenderSystem.AssertOnRenderThread();
        ++_references;
    }

    public static ConvertResult CompileShader(ProgramType type, string name, Stream stream, string str2)
    {
        RenderSystem.AssertOnRenderThread();
        var result = InternalCompileShader(type, name, stream, str2, Preprocessor);
        type.Programs[name] = result;
        return result;
    }

    private class PreprocessorInstance : GlslPreprocessor
    {
        public override string ApplyImport(bool flag, string path) => "#error Import statement not supported";
    }
}