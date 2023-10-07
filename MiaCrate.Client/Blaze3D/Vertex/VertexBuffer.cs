using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using OpenTK.Mathematics;
using Veldrid;

namespace MiaCrate.Client;

public class VertexBuffer : IDisposable
{
    private DeviceBuffer? _vertexBufferId;
    private DeviceBuffer? _indexBufferId;
    private DeviceBuffer? _arrayObjectId;
    private VertexFormat? _format;
    private RenderSystem.AutoStorageIndexBuffer? _sequencialIndices;
    private int _indexCount;
    private VertexFormat.IndexType? _indexType;
    private VertexFormat.Mode? _mode;

    public bool IsInvalid => _arrayObjectId?.IsDisposed ?? true;
    
    public VertexBuffer()
    {
        
    }
    
    public void Upload(BufferBuilder.RenderedBuffer renderedBuffer)
    {
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
            if (_indexBufferId == null || _indexBufferId.SizeInBytes < buffer.Length)
            {
                var bufferDesc = new BufferDescription((uint) buffer.Length, BufferUsage.IndexBuffer);
                _indexBufferId?.Dispose();
                _indexBufferId = GlStateManager.ResourceFactory.CreateBuffer(bufferDesc);
            }
            
            GlStateManager.SetIndexBuffer(_indexBufferId, drawState.IndexType.Format);
            RenderSystem.BufferData(_indexBufferId, buffer);
            return null;
        }

        var b = RenderSystem.GetSequentialBuffer(drawState.Mode);
        b.Bind(drawState.IndexCount);

        return b;
    }

    private VertexFormat UploadVertexBuffer(BufferBuilder.DrawState drawState, ReadOnlySpan<byte> buffer)
    {
        // var boundToVertex = false;
        if (drawState.Format != _format)
        {
            _format?.ClearBufferState();

            // GlStateManager.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            // drawState.Format.SetupBufferState();
            // boundToVertex = true;
        }

        if (!drawState.IndexOnly)
        {
            // if (!boundToVertex)
            // {
            //     GlStateManager.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            // }
            
            if (_vertexBufferId == null || _vertexBufferId.SizeInBytes < buffer.Length)
            {
                var bufferDesc = new BufferDescription((uint) buffer.Length, BufferUsage.VertexBuffer);
                _vertexBufferId?.Dispose();
                _vertexBufferId = GlStateManager.ResourceFactory.CreateBuffer(bufferDesc);
            }
            
            RenderSystem.BufferData(_vertexBufferId, buffer);
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
            shaderInstance.SetSampler($"Sampler{i}", texture!);
        }
        
        shaderInstance.ModelViewMatrix?.Set(modelViewMatrix);
        shaderInstance.ProjectionMatrix?.Set(projectionMatrix);
        shaderInstance.InverseViewRotationMatrix?.Set(RenderSystem.InverseViewRotationMatrix);
        shaderInstance.ColorModulator?.Set(RenderSystem.ShaderColor);

        var window = Game.Instance.Window;
        shaderInstance.ScreenSize?.Set((float) window.Width, window.Height);
        
        shaderInstance.Apply();
        
        // Should only be called after upload!
        // _mode and _indexType are always set to notnull after upload
        if (_mode == null || _indexType == null)
            throw new InvalidOperationException("Upload the buffer first!");
        
        shaderInstance.SetupPipeline(_mode.Type);
        InternalDraw();
        shaderInstance.Clear();
    }
    
    public void Draw()
    {
        // Should only be called after upload!
        // _mode and _indexType are always set to notnull after upload
        if (_mode == null || _indexType == null)
            throw new InvalidOperationException("Upload the buffer first!");

        var blitShader = Game.Instance.GameRenderer.BlitShader;
        blitShader.SetupPipeline(_mode.Type);
        InternalDraw();
    }

    private void InternalDraw()
    {
        var cl = GlStateManager.CommandList;
        GlStateManager.BindVertexBuffer(_vertexBufferId!);
        cl.DrawIndexed((uint) _indexCount);
        GlStateManager.SubmitCommands();
    }
    
    public void Dispose()
    {
    }

    public void Bind()
    {
        BufferUploader.Invalidate();
    }

    public static void Unbind()
    {
        BufferUploader.Invalidate();
        // GlStateManager.BindVertexArray(0);
    }
}