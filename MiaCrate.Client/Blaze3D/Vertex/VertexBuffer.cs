using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MiaCrate.Client;

public class VertexBuffer : IDisposable
{
    private int _vertexBufferId;
    private int _indexBufferId;
    private int _arrayObjectId;
    private readonly BufferUsageHint _usage;
    private VertexFormat? _format;
    private RenderSystem.AutoStorageIndexBuffer? _sequencialIndices;
    private int _indexCount;
    private VertexFormat.IndexType _indexType;
    private VertexFormat.Mode _mode;

    public bool IsInvalid => _arrayObjectId == -1;
    
    public VertexBuffer(BufferUsageHint hint)
    {
        if (hint is not (BufferUsageHint.DynamicDraw or BufferUsageHint.StaticDraw))
            throw new ArgumentOutOfRangeException(nameof(hint));

        _usage = hint;
        
        RenderSystem.AssertOnRenderThread();
        _vertexBufferId = GlStateManager.GenBuffers();
        GlStateManager.ObjectLabel(ObjectLabelIdentifier.Buffer, _vertexBufferId, $"Vertex Buffer #{_vertexBufferId}");
        
        _indexBufferId = GlStateManager.GenBuffers();
        GlStateManager.ObjectLabel(ObjectLabelIdentifier.Buffer, _indexBufferId, $"Index Buffer #{_indexBufferId}");
        
        _arrayObjectId = GlStateManager.GenVertexArrays();
        GlStateManager.ObjectLabel(ObjectLabelIdentifier.VertexArray, _arrayObjectId, $"Vertex Array #{_arrayObjectId}");
    }
    
    public void Upload(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        if (IsInvalid) return;
        RenderSystem.AssertOnRenderThread();

        using (renderedBuffer)
        {
            var drawState = renderedBuffer.DrawState;
            _format = UploadVertexBuffer(drawState, renderedBuffer.VertexBuffer);
            _sequencialIndices = UploadIndexBuffer(drawState, renderedBuffer.IndexBuffer);
            _indexCount = drawState.IndexCount;
            _indexType = drawState.IndexType;
            _mode = drawState.Mode;
        }
    }

    private RenderSystem.AutoStorageIndexBuffer? UploadIndexBuffer(BufferBuilder.DrawState drawState, ReadOnlySpan<byte> buffer)
    {
        if (!drawState.SequentialIndex)
        {
            GlStateManager.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferId);
            RenderSystem.BufferData(BufferTarget.ElementArrayBuffer, buffer, _usage);
            return null;
        }

        var b = RenderSystem.GetSequentialBuffer(drawState.Mode);
        if (b != _sequencialIndices || !b.HasStorage(drawState.IndexCount))
        {
            b.Bind(drawState.IndexCount);
        }

        return b;
    }

    private VertexFormat UploadVertexBuffer(BufferBuilder.DrawState drawState, ReadOnlySpan<byte> buffer)
    {
        var boundToVertex = false;
        if (drawState.Format != _format)
        {
            _format?.ClearBufferState();

            GlStateManager.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            drawState.Format.SetupBufferState();
            boundToVertex = true;
        }

        if (!drawState.IndexOnly)
        {
            if (!boundToVertex)
            {
                GlStateManager.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            }

            RenderSystem.BufferData(BufferTarget.ArrayBuffer, buffer, _usage);
        }

        return drawState.Format;
    }

    public void DrawWithShader(Matrix4 modelViewMatrix, Matrix4 projectionMatrix, ShaderInstance shaderInstance)
    {
        RenderSystem.EnsureOnRenderThread(() => 
            InternalDrawWithShader(modelViewMatrix, projectionMatrix, shaderInstance));
    }

    private void InternalDrawWithShader(Matrix4 modelViewMatrix, Matrix4 projectionMatrix, ShaderInstance shaderInstance)
    {
        for (var i = 0; i < 12; i++)
        {
            var texture = RenderSystem.GetShaderTexture(i);
            shaderInstance.SetSampler($"Sampler{i}", texture);
        }
        
        shaderInstance.ModelViewMatrix?.Set(modelViewMatrix);
        shaderInstance.ProjectionMatrix?.Set(projectionMatrix);
        shaderInstance.InverseViewRotationMatrix?.Set(RenderSystem.InverseViewRotationMatrix);
        shaderInstance.ColorModulator?.Set(RenderSystem.ShaderColor);

        var window = Game.Instance.Window;
        shaderInstance.ScreenSize?.Set((float) window.Width, window.Height);
        
        shaderInstance.Apply();
        Draw();
        shaderInstance.Clear();
    }

    public void Draw()
    {
        RenderSystem.DrawElements(_mode.Type, _indexCount, _indexType.GlType);
    }
    
    public void Dispose()
    {
    }

    public void Bind()
    {
        BufferUploader.Invalidate();
        GlStateManager.BindVertexArray(_arrayObjectId);
    }

    public static void Unbind()
    {
        BufferUploader.Invalidate();
        GlStateManager.BindVertexArray(0);
    }
}