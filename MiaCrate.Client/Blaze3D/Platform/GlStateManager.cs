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
    
    public static void AttachShader(int i, int j)
    {
        
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

    public static void TexMinFilter(TextureTarget target, TextureMinFilter filter) =>
        TexParameter(target, TextureParameterName.TextureMinFilter, (int) filter);
    public static void TexMagFilter(TextureTarget target, TextureMagFilter filter) => 
        TexParameter(target, TextureParameterName.TextureMagFilter, (int) filter);
    public static void TexWrapS(TextureTarget target, TextureWrapMode mode) =>
        TexParameter(target, TextureParameterName.TextureWrapS, (int) mode);
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
}