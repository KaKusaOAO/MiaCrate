using System.Diagnostics.CodeAnalysis;
using MiaCrate.Client.Systems;
using Veldrid;

namespace MiaCrate.Client.Platform;

public static class GlStateManager
{
    private static readonly Dictionary<string, bool> _funcSupportCache = new();
    private static readonly BlendState _blend = new();
    private static readonly DepthState _depth = new();
    private static readonly CullState _cull = new();
    private static readonly PolygonOffsetState _polyOffset = new();
    private static readonly ColorLogicState _colorLogic = new();
    private static readonly ScissorState _scissor = new();
    private static readonly ColorMask _colorMask = new();

    private static readonly TextureState[] _textures =
        Enumerable.Range(0, 12).Select(i => new TextureState()).ToArray();

    private static GraphicsDevice? _device;
    private static ResourceFactory? _resourceFactory;
    private static CommandList? _commandList;
    private static GraphicsPipelineDescription _pipelineDescription;
    private static Veldrid.Pipeline? _pipeline;
    private static bool _pipelineDirty = true;

    public static GraphicsDevice Device => 
        _device ?? throw new InvalidOperationException("Device not ready");
    
    public static ResourceFactory ResourceFactory =>
        _resourceFactory ?? throw new InvalidOperationException("Device not ready");

    public static CommandList CommandList =>
        _commandList ?? throw new InvalidOperationException("Device not ready");

    public static void Init(GraphicsDevice device)
    {
        _device = device;
        _resourceFactory = device.ResourceFactory;
        _commandList = _resourceFactory.CreateCommandList();

        _pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = new BlendStateDescription
            {
                BlendFactor = RgbaFloat.White,
                AttachmentStates = new[]
                {
                    new BlendAttachmentDescription
                    {
                        BlendEnabled = false,
                        SourceColorFactor = BlendFactor.One,
                        DestinationColorFactor = BlendFactor.Zero,
                        ColorFunction = BlendFunction.Add,
                        SourceAlphaFactor = BlendFactor.One,
                        DestinationAlphaFactor = BlendFactor.Zero,
                        AlphaFunction = BlendFunction.Add,
                        ColorWriteMask = ColorWriteMask.All
                    }
                }
            },
            RasterizerState = new RasterizerStateDescription
            {
                CullMode = FaceCullMode.Back,
                ScissorTestEnabled = false
            }
        };
    }

    private static void BuildPipelineIfDirty()
    {
        if (_pipeline != null && !_pipelineDirty) return;
        
        _pipeline?.Dispose();
        _pipeline = _resourceFactory!.CreateGraphicsPipeline(_pipelineDescription);
        _pipelineDirty = false;
    }
    
    public static void EnableDepthTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _depth.State.Enable();

        if (!_pipelineDescription.DepthStencilState.DepthTestEnabled)
        {
            _pipelineDescription.DepthStencilState.DepthTestEnabled = true;
            _pipelineDirty = true;
        }
    }

    public static void DisableDepthTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _depth.State.Disable();
        
        if (_pipelineDescription.DepthStencilState.DepthTestEnabled)
        {
            _pipelineDescription.DepthStencilState.DepthTestEnabled = false;
            _pipelineDirty = true;
        }
    }

    public static void DepthFunc(ComparisonKind func)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        if (func == _depth.DepthFunction) return;
        _depth.DepthFunction = func;
        
        if (_pipelineDescription.DepthStencilState.DepthComparison != func)
        {
            _pipelineDescription.DepthStencilState.DepthComparison = func;
            _pipelineDirty = true;
        }
    }

    public static void DisableBlend()
    {
        RenderSystem.AssertOnRenderThread();
        _blend.State.Disable();

        var state = _pipelineDescription.BlendState.AttachmentStates[0];
        if (state.BlendEnabled)
        {
            state.BlendEnabled = false;
            _pipelineDirty = true;
        }
    }

    public static void EnableBlend()
    {
        RenderSystem.AssertOnRenderThread();
        _blend.State.Enable();
        
        var state = _pipelineDescription.BlendState.AttachmentStates[0];
        if (state.BlendEnabled)
        {
            state.BlendEnabled = true;
            _pipelineDirty = true;
        }
    }

    public static void BlendFunc(BlendFactor sourceFactor, BlendFactor destFactor) => 
        BlendFuncSeparate(sourceFactor, destFactor, sourceFactor, destFactor);

    public static void BlendFuncSeparate(BlendFactor sourceRgb, BlendFactor destRgb,
        BlendFactor sourceAlpha, BlendFactor destAlpha)
    {
        RenderSystem.AssertOnRenderThread();
        if (sourceRgb == _blend.SrcRgb && destRgb == _blend.DstRgb &&
            sourceAlpha == _blend.SrcAlpha && destAlpha == _blend.DstAlpha) return;

        _blend.SrcRgb = sourceRgb;
        _blend.DstRgb = destRgb;
        _blend.SrcAlpha = sourceAlpha;
        _blend.DstAlpha = destAlpha;

        var state = _pipelineDescription.BlendState.AttachmentStates[0];
        if (state.SourceColorFactor == sourceRgb && state.DestinationColorFactor == destRgb &&
            state.SourceAlphaFactor == sourceAlpha && state.DestinationAlphaFactor == destAlpha) return;
        
        state.SourceColorFactor = sourceRgb;
        state.DestinationColorFactor = destRgb;
        state.SourceAlphaFactor = sourceAlpha;
        state.DestinationAlphaFactor = destAlpha;
        _pipelineDirty = true;
    }

    public static void BlendEquation(BlendFunction func)
    {
        RenderSystem.AssertOnRenderThread();

        var state = _pipelineDescription.BlendState.AttachmentStates[0];
        if (state.ColorFunction == func && state.AlphaFunction == func) return;
        
        state.ColorFunction = func;
        state.AlphaFunction = func;
        _pipelineDirty = true;
    }

    public static void UseProgram(ShaderSetDescription shaderSet)
    {
        if (_pipelineDescription.ShaderSet.Equals(shaderSet)) return;
        _pipelineDescription.ShaderSet = shaderSet;
        _pipelineDirty = true;
    }

    public static void Viewport(int x, int y, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        ViewportState.X = x;
        ViewportState.Y = y;
        ViewportState.Width = width;
        ViewportState.Height = height;
        
        _commandList!.SetViewport(0, 
            new Viewport(ViewportState.X, ViewportState.Y, ViewportState.Width, ViewportState.Height, 
                0, 1));
    }
    
    public static void ClearDepth(double depth)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _commandList!.ClearDepthStencil((float) depth);
    }

    public static void BindVertexBuffer(DeviceBuffer buffer)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _commandList!.SetVertexBuffer(0, buffer);
    }
    
    public static void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _commandList!.SetIndexBuffer(buffer, format);
    }

    public static void BufferData(DeviceBuffer buffer, ReadOnlySpan<byte> span)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        unsafe
        {
            fixed (byte* ptr = span)
            {
                _commandList!.UpdateBuffer(buffer, 0, (IntPtr) ptr, (uint) span.Length);
            }
        }
    }
    
    public static void DrawElements(PrimitiveTopology mode, int count)
    {
        RenderSystem.AssertOnRenderThread();
        if (_pipelineDescription.PrimitiveTopology != mode)
        {
            _pipelineDescription.PrimitiveTopology = mode;
            _pipelineDirty = true;
        } 
        
        BuildPipelineIfDirty();
        
        _commandList!.SetPipeline(_pipeline);
        // TODO: Bind resource sets
        // _commandList!.SetGraphicsResourceSet(0, );
        // _commandList!.SetGraphicsResourceSet(1, );

        _commandList.Draw((uint) count);
    }

    public static void EnableCull()
    {
        RenderSystem.AssertOnRenderThread();
        _cull.State.Enable();

        if (_pipelineDescription.RasterizerState.CullMode != FaceCullMode.None) return;
        _pipelineDescription.RasterizerState.CullMode = _cull.Mode;
        _pipelineDirty = true;
    }

    public static void DepthMask(bool flag)
    {
        RenderSystem.AssertOnRenderThread();
        if (flag == _depth.EnableMask) return;
        _depth.EnableMask = flag;

        if (_pipelineDescription.DepthStencilState.DepthWriteEnabled == flag) return;
        _pipelineDescription.DepthStencilState.DepthWriteEnabled = flag;
        _pipelineDirty = true;
    }
    
    public static void DisableCull()
    {
        RenderSystem.AssertOnRenderThread();
        _cull.State.Disable();

        if (_pipelineDescription.RasterizerState.CullMode == FaceCullMode.None) return;
        _pipelineDescription.RasterizerState.CullMode = FaceCullMode.None;
        _pipelineDirty = true;
    }

    public static void ColorMask(bool red, bool green, bool blue, bool alpha)
    {
        RenderSystem.AssertOnRenderThread();
        if (red == _colorMask.Red && green == _colorMask.Green && blue == _colorMask.Blue &&
            alpha == _colorMask.Alpha) return;
        
        _colorMask.Red = red;
        _colorMask.Green = green;
        _colorMask.Blue = blue;
        _colorMask.Alpha = alpha;

        var state = _pipelineDescription.BlendState.AttachmentStates[0];
        
        var mask = ColorWriteMask.None;
        if (red) mask |= ColorWriteMask.Red;
        if (green) mask |= ColorWriteMask.Green;
        if (blue) mask |= ColorWriteMask.Blue;
        if (alpha) mask |= ColorWriteMask.Alpha;
        
        if (!state.ColorWriteMask.HasValue)
        {
            state.ColorWriteMask = mask;
            _pipelineDirty = true;
        }
        else
        {
            var oldMask = state.ColorWriteMask.Value;
            if (oldMask == mask) return;

            state.ColorWriteMask = mask;
            _pipelineDirty = true;
        }
    }

    public static void PushDebugGroup(string message)
    {
        RenderSystem.AssertOnRenderThread();
        _commandList!.PushDebugGroup(message);
    }
    
    public static void PopDebugGroup()
    {
        RenderSystem.AssertOnRenderThread();
        _commandList!.PopDebugGroup();
    }

    public static void WrapWithDebugGroup(string message, Action action)
    {
        RenderSystem.AssertOnRenderThread();
        PushDebugGroup(message);
        try
        {
            action();
        }
        finally
        {
            PopDebugGroup();
        }
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public static void PolygonOffset(float factor, float units)
    {
        RenderSystem.AssertOnRenderThread();
        if (factor == _polyOffset.Factor && units == _polyOffset.Units) return;

        _polyOffset.Factor = factor;
        _polyOffset.Units = units;
        // TODO: GL.PolygonOffset(factor, units);
    }
    
    public static void EnablePolygonOffset()
    {
        RenderSystem.AssertOnRenderThread();
        _polyOffset.FillState.Enable();
    }
    
    public static void DisablePolygonOffset()
    {
        RenderSystem.AssertOnRenderThread();
        _polyOffset.FillState.Disable();
    }
    
    public static void EnableScissorTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _scissor.State.Enable();

        if (_pipelineDescription.RasterizerState.ScissorTestEnabled) return;
        _pipelineDescription.RasterizerState.ScissorTestEnabled = true;
        _pipelineDirty = true;
    }

    public static void ScissorBox(int x, int y, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _commandList!.SetScissorRect(0, (uint) x, (uint) y, (uint) width, (uint) height);
    }

    public static void DisableScissorTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _scissor.State.Disable();
        
        if (!_pipelineDescription.RasterizerState.ScissorTestEnabled) return;
        _pipelineDescription.RasterizerState.ScissorTestEnabled = false;
        _pipelineDirty = true;
    }

    // Why is this needed?
    private static class ViewportState
    {
        public static int X { get; set; }
        public static int Y { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
    }
}