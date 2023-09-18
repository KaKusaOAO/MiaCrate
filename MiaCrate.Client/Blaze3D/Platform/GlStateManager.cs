using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using All = OpenTK.Graphics.OpenGL.All;

namespace MiaCrate.Client.Platform;

public static class GlStateManager
{
    private static readonly BlendState _blend = new();
    private static readonly DepthState _depth = new();
    private static readonly CullState _cull = new();
    private static readonly PolygonOffsetState _polyOffset = new();
    private static readonly ColorLogicState _colorLogic = new();
    private static readonly ScissorState _scissor = new();
    private const int TextureUnit = (int) OpenTK.Graphics.OpenGL4.TextureUnit.Texture0;
    private static int _activeTexture;
    private static readonly TextureState[] _textures = 
        Enumerable.Range(0, 12).Select(i => new TextureState()).ToArray();
    
    public static void AttachShader(int program, int shader)
    {
        RenderSystem.AssertOnRenderThread();
        GL.AttachShader(program, shader);
    }

    public static int GetInteger(GetPName name)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        return GL.GetInteger(name);
    }

    public static string GetString(StringName name)
    {
        RenderSystem.AssertOnRenderThread();
        return GL.GetString(name);
    }

    public static void EnableVertexAttribArray(int index)
    {
        RenderSystem.AssertOnRenderThread();
        GL.EnableVertexAttribArray(index);
    }
    
    public static void DisableVertexAttribArray(int index)
    {
        RenderSystem.AssertOnRenderThread();
        GL.DisableVertexAttribArray(index);
    }
    
    public static void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, IntPtr ptr)
    {
        RenderSystem.AssertOnRenderThread();
        GL.VertexAttribPointer(index, size, type, normalized, stride, ptr);
    }
    
    public static void VertexAttribIPointer(int index, int size, VertexAttribIntegerType type, int stride, IntPtr ptr)
    {
        RenderSystem.AssertOnRenderThread();
        GL.VertexAttribIPointer(index, size, type, stride, ptr);
    }
    
    public static void VertexAttribIPointer(int index, int size, VertexAttribPointerType type, int stride, IntPtr ptr)
    {
        var name = Enum.GetName(type);
        if (Enum.TryParse<VertexAttribIntegerType>(name, out var t))
        {
            VertexAttribIPointer(index, size, t, stride, ptr);
            return;
        }
        
        Logger.Warn($"Unknown {nameof(VertexAttribIntegerType)}: {type}");
    }

    public static void GenTextures(int[] textures)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.GenTextures(textures.Length, textures);
    }

    public static int[] GenTextures(int count)
    {
        var textures = new int[count];
        GenTextures(textures);
        return textures;
    }

    public static int GenTexture()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.GenTextures(1, out int result);
        return result;
    }

    public static void DeleteTextures(int[] textures)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        foreach (var state in _textures)
        {
            foreach (var texture in textures)
            {
                if (state.Binding == texture) 
                    state.Binding = -1;
            }
        }
        
        GL.DeleteTextures(textures.Length, textures);
    }
    
    public static void DeleteTexture(int texture)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.DeleteTextures(1, ref texture);
        
        foreach (var state in _textures)
        {
            if (state.Binding == texture) 
                state.Binding = -1;
        }
    }

    public static void TexParameter(TextureTarget target, TextureParameterName pName, int value)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.TexParameter(target, pName, value);;
    }
    
    public static void TexParameter(TextureTarget target, TextureParameterName pName, float value)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.TexParameter(target, pName, value);
    }

    /// <summary>
    /// For quick check, this applies to 10241 (<see cref="TextureParameterName.TextureMinFilter"/>).
    /// </summary>
    public static void TexMinFilter(TextureTarget target, TextureMinFilter filter) =>
        TexParameter(target, TextureParameterName.TextureMinFilter, (int) filter);
    
    /// <summary>
    /// For quick check, this applies to 10240 (<see cref="TextureParameterName.TextureMagFilter"/>).
    /// </summary>
    public static void TexMagFilter(TextureTarget target, TextureMagFilter filter) => 
        TexParameter(target, TextureParameterName.TextureMagFilter, (int) filter);
    
    /// <summary>
    /// For quick check, this applies to 34892 (<see cref="TextureParameterName.TextureCompareMode"/>).
    /// </summary>
    public static void TexCompareMode(TextureTarget target, TextureCompareMode mode) => 
        TexParameter(target, TextureParameterName.TextureCompareMode, (int) mode);
    
    /// <summary>
    /// For quick check, this applies to 10242 (<see cref="TextureParameterName.TextureWrapS"/>).
    /// </summary>
    public static void TexWrapS(TextureTarget target, TextureWrapMode mode) =>
        TexParameter(target, TextureParameterName.TextureWrapS, (int) mode);
    
    /// <summary>
    /// For quick check, this applies to 10243 (<see cref="TextureParameterName.TextureWrapT"/>).
    /// </summary>
    public static void TexWrapT(TextureTarget target, TextureWrapMode mode) => 
        TexParameter(target, TextureParameterName.TextureWrapT, (int) mode);

    public static void PixelStore(PixelStoreParameter pName, int param)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.PixelStore(pName, param);
    }

    public static void TexSubImage2D(TextureTarget target, int level, int xOffset, int yOffset, int width, int height,
        PixelFormat format, PixelType type, IntPtr pixels)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, format, type, pixels);
    }

    public static void GetTexImage(TextureTarget target, int level, PixelFormat format, PixelType type, IntPtr pixels)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetTexImage(target, level, format, type, pixels);
    }

    public static void BindTexture(int texture)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        if (texture != _textures[_activeTexture].Binding)
        {
            _textures[_activeTexture].Binding = texture;
            GL.BindTexture(TextureTarget.Texture2D, texture);
        }
    }

    private static void InternalActiveTexture(int textureUnit)
    {
        RenderSystem.AssertOnRenderThread();
        GL.ActiveTexture((TextureUnit) textureUnit);
    }

    public static int GetActiveTexture() => _activeTexture + TextureUnit;

    public static void ActiveTexture(int textureUnit)
    {
        RenderSystem.AssertOnRenderThread();
        if (_activeTexture != textureUnit - TextureUnit)
        {
            _activeTexture = textureUnit - TextureUnit;
            InternalActiveTexture(textureUnit);
        }
    }

    public static void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalFormat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.TexImage2D(target, level, internalFormat, width, height, border, format, type, pixels);
    }

    public static void EnableDepthTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _depth.State.Enable();
    }
    
    public static void DisableDepthTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _depth.State.Disable();
    }

    public static void DepthFunc(DepthFunction func)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        if (func == _depth.DepthFunction) return;
        
        _depth.DepthFunction = func;
        GL.DepthFunc(func);
    }

    public static void DisableBlend()
    {
        RenderSystem.AssertOnRenderThread();
        _blend.State.Disable();
    }
    
    public static void EnableBlend()
    {
        RenderSystem.AssertOnRenderThread();
        _blend.State.Enable();
    }

    public static void BlendFunc(BlendingFactorSrc sourceFactor, BlendingFactorDest destFactor)
    {
        RenderSystem.AssertOnRenderThread();
        if (sourceFactor == _blend.SrcRgb && destFactor == _blend.DstRgb) return;
        
        _blend.SrcRgb = sourceFactor;
        _blend.DstRgb = destFactor;
        GL.BlendFunc((BlendingFactor) sourceFactor, (BlendingFactor) destFactor);
    }
    
    public static void BlendFuncSeparate(BlendingFactorSrc sourceRgb, BlendingFactorDest destRgb, BlendingFactorSrc sourceAlpha, BlendingFactorDest destAlpha)
    {
        RenderSystem.AssertOnRenderThread();
        if (sourceRgb == _blend.SrcRgb && destRgb == _blend.DstRgb && 
            sourceAlpha == _blend.SrcAlpha && destAlpha == _blend.DstAlpha) return;
        
        _blend.SrcRgb = sourceRgb;
        _blend.DstRgb = destRgb;
        _blend.SrcAlpha = sourceAlpha;
        _blend.DstAlpha = destAlpha;
        GL.BlendFuncSeparate(sourceRgb, destRgb, sourceAlpha, destAlpha);
    }

    public static void Uniform1(int location, int[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform1(location, 1, buffer);
    }
    
    public static void Uniform1(int location, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform1(location, 1, buffer);
    }

    public static void Uniform1(int location, int value)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform1(location, value);
    }
    
    public static void Uniform2(int location, int[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform2(location, 1, buffer);
    }
    
    public static void Uniform2(int location, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform2(location, 1, buffer);
    }
    
    public static void Uniform3(int location, int[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform3(location, 1, buffer);
    }
    
    public static void Uniform3(int location, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform3(location, 1, buffer);
    }
    
    public static void Uniform4(int location, int[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform4(location, 1, buffer);
    }
    
    public static void Uniform4(int location, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.Uniform4(location, 1, buffer);
    }
    
    public static void UniformMatrix2(int location, bool transpose, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.UniformMatrix2(location, 1, transpose, buffer);
    }
    
    public static void UniformMatrix3(int location, bool transpose, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.UniformMatrix3(location, 1, transpose, buffer);
    }
    
    public static void UniformMatrix4(int location, bool transpose, float[] buffer)
    {
        RenderSystem.AssertOnRenderThread();
        GL.UniformMatrix4(location, 1, transpose, buffer);
    }
    
    public static void BlendEquation(BlendEquationMode func)
    {
        RenderSystem.AssertOnRenderThread();
        GL.BlendEquation(func);
    }

    public static int CreateShader(ShaderType type)
    {
        RenderSystem.AssertOnRenderThread();
        return GL.CreateShader(type);
    }

    public static void ShaderSource(int shader, List<string> list)
    {
        RenderSystem.AssertOnRenderThread();
        
        var source = string.Join('\n', list);
        GL.ShaderSource(shader, source);
    }

    public static void CompileShader(int shader)
    {
        RenderSystem.AssertOnRenderThread();
        GL.CompileShader(shader);
    }

    public static int GetShaderI(int shader, ShaderParameter pName)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetShader(shader, pName, out var result);
        return result;
    }

    public static string GetShaderInfoLog(int shader, int maxLength)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetShaderInfoLog(shader, maxLength, out _, out var result);
        return result;
    }
    
    public static string GetShaderInfoLog(int shader)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetShaderInfoLog(shader, out var result);
        return result;
    }

    public static void UseProgram(int program)
    {
        RenderSystem.AssertOnRenderThread();
        GL.UseProgram(program);
    }

    public static void DeleteShader(int shader)
    {
        RenderSystem.AssertOnRenderThread();
        GL.DeleteSampler(shader);
    }

    public static void DeleteProgram(int program)
    {
        RenderSystem.AssertOnRenderThread();
        GL.DeleteProgram(program);
    }

    public static int CreateProgram()
    {
        RenderSystem.AssertOnRenderThread();
        return GL.CreateProgram();
    }

    public static void LinkProgram(int program)
    {
        RenderSystem.AssertOnRenderThread();
        GL.LinkProgram(program);
    }

    public static int GetProgramI(int program, GetProgramParameterName pName)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetProgram(program, pName, out var result);
        return result;
    }
    
    public static string GetProgramInfoLog(int program, int maxLength)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetProgramInfoLog(program, maxLength, out _, out var result);
        return result;
    }
    
    public static string GetProgramInfoLog(int program)
    {
        RenderSystem.AssertOnRenderThread();
        GL.GetProgramInfoLog(program, out var result);
        return result;
    }

    public static int GetUniformLocation(int program, string name)
    {
        RenderSystem.AssertOnRenderThread();
        return GL.GetUniformLocation(program, name);
    }

    public static void BindAttribLocation(int program, int index, string name)
    {
        RenderSystem.AssertOnRenderThread();
        GL.BindAttribLocation(program, index, name);
    }

    public static void BindFramebuffer(FramebufferTarget target, int framebuffer)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.BindFramebuffer(target, framebuffer);
    }

    public static void DeleteFramebuffers(int framebuffer)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.DeleteFramebuffers(1, ref framebuffer);
    }

    public static int GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pName)
    {
        RenderSystem.AssertInInitPhase();
        GL.GetTexLevelParameter(target, level, pName, out int result);
        return result;
    }

    public static int GenFramebuffers()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.GenFramebuffers(1, out int result);
        return result;
    }

    public static void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textureTarget, int texture, int level)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.FramebufferTexture2D(target, attachment, textureTarget, texture, level);
    }

    public static FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        return GL.CheckFramebufferStatus(target);
    }

    public static void Viewport(int x, int y, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        ViewportState.X = x;
        ViewportState.Y = y;
        ViewportState.Width = width;
        ViewportState.Height = height;
        GL.Viewport(x, y, width, height);
    }

    public static void ClearColor(float red, float green, float blue, float alpha)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.ClearColor(red, green, blue, alpha);
    }

    public static void Clear(ClearBufferMask mask, bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.Clear(mask);

        if (clearError) GetError();
    }

    public static ErrorCode GetError()
    {
        RenderSystem.AssertOnRenderThread();
        return GL.GetError();
    }

    public static void ClearDepth(double depth)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.ClearDepth(depth);
    }

    public static void BlitFramebuffer(
        int srcX0, int srcY0, int srcX1, int srcY1, 
        int dstX0, int dstY0, int dstX1, int dstY1, 
        ClearBufferMask clearMask, BlitFramebufferFilter filter)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, clearMask, filter);
    }

    public static int GenBuffers()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.GenBuffers(1, out int result);
        return result;
    }
    
    public static int GenVertexArrays()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.GenVertexArrays(1, out int result);
        return result;
    }

    public static void BindBuffer(BufferTarget target, int buffer)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GL.BindBuffer(target, buffer);
    }

    public static void BufferData(BufferTarget target, ReadOnlySpan<byte> span, BufferUsageHint usage)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        unsafe
        {
            fixed (byte* ptr = span)
            {
                GL.BufferData(target, span.Length, (IntPtr) ptr, usage);
            }
        }
    }

    public static void DrawElements(PrimitiveType mode, int count, DrawElementsType type, IntPtr indices)
    {
        RenderSystem.AssertOnRenderThread();
        GL.DrawElements(mode, count, type, indices);
    }

    // Why is this needed?
    private static class ViewportState
    {
        public static int X { get; set; }
        public static int Y { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
    }

    public static void EnableCull()
    {
        RenderSystem.AssertOnRenderThread();
        _cull.State.Enable();
    }
}