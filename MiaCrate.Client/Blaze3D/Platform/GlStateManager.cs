using System.Diagnostics.CodeAnalysis;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using Mochi.Extensions;
using Mochi.Utils;
using Veldrid;
using Veldrid.Utilities;

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
    
    private static readonly Dictionary<GraphicsPipelineDescription, Veldrid.Pipeline> _pipelineCache = new();
    private static readonly TextureState[] _textures =
        Enumerable.Range(0, 12).Select(i => new TextureState()).ToArray();

    private static GraphicsPipelineDescription _pipelineDescription;
    private static Veldrid.Pipeline? _pipeline;
    private static bool _pipelineDirty = true;
    private static DeviceBuffer? _indexBuffer;
    private static IndexFormat _indexFormat;
    private static Framebuffer? _framebuffer;

    public static GraphicsDevice Device { get; private set; } = null!;
    
    public static ResourceFactory ResourceFactory { get; private set; } = null!;
    public static DisposeCollectorResourceFactory DisposableResourceFactory { get; private set; } = null!;

    public static CommandList CommandList { get; private set; } = null!;
    public static CommandList BufferCommandList { get; private set; } = null!;

    private static bool _beganCommand;
    private static bool _beganBufferCommand;

    public static void SubmitCommands()
    {
        FlushBufferCommands();

        if (_beganCommand)
        {
            CommandList.End();
            Device.SubmitCommands(CommandList);
            Device.WaitForIdle();
        }

        _beganCommand = false;
    }

    public static CommandList EnsureCommandBegan()
    {
        if (_beganCommand) return CommandList;
        
        CommandList.Begin();
        _beganCommand = true;
        return CommandList;
    }

    public static CommandList EnsureFramebufferSet()
    {
        EnsureCommandBegan();
        BindOutput(_framebuffer ?? Device.SwapchainFramebuffer);
        CommandList.SetFramebuffer(_framebuffer);
        return CommandList;
    }
    
    public static CommandList EnsureFramebufferSet(Framebuffer fb)
    {
        EnsureCommandBegan();
        BindOutput(fb);
        CommandList.SetFramebuffer(_framebuffer);
        return CommandList;
    }

    public static CommandList EnsureBufferCommandBegan()
    {
        if (_beganBufferCommand) return BufferCommandList;
        BufferCommandList.Begin();
        _beganBufferCommand = true;
        return BufferCommandList;
    }

    public static void FlushBufferCommands()
    {
        if (!_beganBufferCommand) return;
        
        BufferCommandList.End();
        Device.SubmitCommands(BufferCommandList);
        Device.WaitForIdle();
        
        _beganBufferCommand = false;
    }

    public static void Init(GraphicsDevice device)
    {
        Device = device;
        ResourceFactory = device.ResourceFactory;
        DisposableResourceFactory = new DisposeCollectorResourceFactory(device.ResourceFactory);
        CommandList = ResourceFactory.CreateCommandList();
        BufferCommandList = ResourceFactory.CreateCommandList();
        Logger.Info($"Graphic backend: {device.BackendType}");

        _pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = new BlendStateDescription
            {
                BlendFactor = new RgbaFloat(),
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
                ScissorTestEnabled = false,
                DepthClipEnabled = true,
                FrontFace = FrontFace.CounterClockwise
            }
        };
    }

    private static void BuildPipelineIfDirty()
    {
        if (_pipeline != null && !_pipelineDirty) return;
        _pipeline = _pipelineCache.ComputeIfAbsent(_pipelineDescription, 
            d => ResourceFactory.CreateGraphicsPipeline(d));
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

        ref var state = ref _pipelineDescription.BlendState.AttachmentStates[0];
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
        
        ref var state = ref _pipelineDescription.BlendState.AttachmentStates[0];
        if (!state.BlendEnabled)
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
        
        ref var state = ref _pipelineDescription.BlendState.AttachmentStates[0];
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

        ref var state = ref _pipelineDescription.BlendState.AttachmentStates[0];
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

    public static void BindOutput(Framebuffer framebuffer)
    {
        if (_framebuffer == framebuffer) return;
        _framebuffer = framebuffer;
        
        if (_pipelineDescription.Outputs.Equals(framebuffer.OutputDescription)) return;
        
        // Incompatible framebuffer. Flush the commands first. 
        SubmitCommands();
        
        EnsureFramebufferSet(framebuffer);
        _pipelineDescription.Outputs = framebuffer.OutputDescription;
        _pipelineDirty = true;
    }

    [Obsolete]
    public static void QueueFramebufferCommand()
    {
        CommandList.SetFramebuffer(_framebuffer);
    }

    public static void Viewport(int x, int y, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        ViewportState.X = x;
        ViewportState.Y = y;
        ViewportState.Width = width;
        ViewportState.Height = height;
        
        CommandList.SetViewport(0, 
            new Viewport(ViewportState.X, ViewportState.Y, ViewportState.Width, ViewportState.Height, 
                0, 1));
    }
    
    public static void ClearDepth(double depth)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        CommandList.ClearDepthStencil((float) depth);
    }

    public static void BindVertexBuffer(DeviceBuffer buffer)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        CommandList.SetVertexBuffer(0, buffer);
    }
    
    public static void BindIndexBuffer(DeviceBuffer buffer, IndexFormat format)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        CommandList.SetIndexBuffer(buffer, format);
    }

    public static void BufferData(DeviceBuffer buffer, ReadOnlySpan<byte> span)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        unsafe
        {
            fixed (byte* ptr = span)
            {
                EnsureCommandBegan();
                CommandList.UpdateBuffer(buffer, 0, (IntPtr) ptr, (uint) span.Length);
            }
        }
    }

    public static void SetPrimitiveTopology(PrimitiveTopology mode)
    {
        if (_pipelineDescription.PrimitiveTopology != mode)
        {
            _pipelineDescription.PrimitiveTopology = mode;
            _pipelineDirty = true;
        }
    }

    public static void SetResourceLayouts(ResourceLayout[] layouts)
    {
        if (_pipelineDescription.ResourceLayouts != layouts)
        {
            _pipelineDescription.ResourceLayouts = layouts;
            _pipelineDirty = true;
        }
    } 

    public static void SetPipeline()
    {
        RenderSystem.AssertOnRenderThread();
        BuildPipelineIfDirty();
        CommandList.SetPipeline(_pipeline);
        
        if (_indexBuffer != null)
            CommandList.SetIndexBuffer(_indexBuffer, _indexFormat);
        
        EnsureFramebufferSet();
    }
    
    public static void DrawElements(int count)
    {
        RenderSystem.AssertOnRenderThread();
        CommandList.Draw((uint) count);
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
        
        ref var state = ref _pipelineDescription.BlendState.AttachmentStates[0];
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
            var oldMask = state.ColorWriteMask!.Value;
            if (oldMask == mask) return;

            state.ColorWriteMask = mask;
            _pipelineDirty = true;
        }
    }

    public static void PushDebugGroup(string message)
    {
        RenderSystem.AssertOnRenderThread();
        EnsureCommandBegan();
        CommandList.PushDebugGroup(message);
    }
    
    public static void PopDebugGroup()
    {
        RenderSystem.AssertOnRenderThread();
        EnsureCommandBegan();
        CommandList.PopDebugGroup();
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
        EnsureFramebufferSet();
        
        // passed: (_fb.Height - (int)height - y)
        var uy = _framebuffer!.Height - height - y;
        CommandList.SetScissorRect(0, (uint) x, (uint) uy, (uint) width, (uint) height);
        SubmitCommands();
    }

    public static void DisableScissorTest()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        _scissor.State.Disable();
        
        if (!_pipelineDescription.RasterizerState.ScissorTestEnabled) return;
        _pipelineDescription.RasterizerState.ScissorTestEnabled = false;
        _pipelineDirty = true;
    }

    public static void SetIndexBuffer(DeviceBuffer indexBuffer, IndexFormat indexFormat)
    {
        _indexBuffer = indexBuffer;
        _indexFormat = indexFormat;
    }

    // Why is this needed?
    private static class ViewportState
    {
        public static int X { get; set; }
        public static int Y { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
    }

    public static void Upload(AbstractTexture texture, int i, int x, int y, int width, int height, IntPtr buffer, int sizeInBytes, Action<IntPtr> consumer)
    {
        RenderSystem.EnsureOnRenderThreadOrInit(() =>
        {
            var tex = texture.Texture!.Texture;
            Device.UpdateTexture(tex, buffer, (uint) sizeInBytes, (uint) x, (uint) y, 0, (uint) width, (uint) height, 1, (uint) i, 0);
            consumer(buffer);
        });
    }
}