using MiaCrate.Client.Systems;
using Mochi.Utils;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public abstract partial class RenderStateShard
{
    public static TransparencyStateShard NoTransparency { get; } =
        new("no_transparency", RenderSystem.DisableBlend, () => { });

    public static TransparencyStateShard AdditiveTransparency { get; } =
        new("additive_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFunc(BlendFactor.One, BlendFactor.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static TransparencyStateShard LightningTransparency { get; } =
        new("lightning_trasnparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFunc(BlendFactor.SourceAlpha, BlendFactor.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static TransparencyStateShard GlintTransparency { get; } =
        new("glint_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendFactor.SourceColor, BlendFactor.One,
                BlendFactor.Zero, BlendFactor.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static TransparencyStateShard CrumblingTransparency { get; } =
        new("crumbling_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendFactor.DestinationColor, BlendFactor.SourceColor,
                BlendFactor.One, BlendFactor.Zero);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static TransparencyStateShard TranslucentTransparency { get; } =
        new("translucent_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendFactor.SourceAlpha, BlendFactor.InverseSourceAlpha,
                BlendFactor.One, BlendFactor.InverseSourceAlpha);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static TextureStateShard BlockSheetMipped { get; } = new(TextureAtlas.LocationBlocks, false, true);
    public static TextureStateShard BlockSheet { get; } = new(TextureAtlas.LocationBlocks, false, false);
    public static EmptyTextureStateShard NoTexture { get; } = new();
    public static LightmapStateShard Lightmap { get; } = new(true);
    public static LightmapStateShard NoLightmap { get; } = new(false);
    public static OverlayStateShard Overlay { get; } = new(true);
    public static OverlayStateShard NoOverlay { get; } = new(false);
    public static CullStateShard Cull { get; } = new(true);
    public static CullStateShard NoCull { get; } = new(false);
    public static DepthTestStateShard NoDepthTest { get; } = new("always", ComparisonKind.Always);
    public static DepthTestStateShard EqualDepthTest { get; } = new("==", ComparisonKind.Equal);
    public static DepthTestStateShard LequalDepthTest { get; } = new("<=", ComparisonKind.LessEqual);
    public static DepthTestStateShard GreaterDepthTest { get; } = new(">", ComparisonKind.Greater);
    public static WriteMaskStateShard ColorDepthWrite { get; } = new(true, true);
    public static WriteMaskStateShard ColorWrite { get; } = new(true, false);
    public static WriteMaskStateShard DepthWrite { get; } = new(false, true);
    public static LayeringStateShard NoLayering { get; } = new("no_layering", () => { }, () => { });
    
    public static LayeringStateShard PolygonOffsetLayering { get; } = new("polygon_offset_layering", () =>
    {
        RenderSystem.PolygonOffset(-1f, -10f);
        RenderSystem.EnablePolygonOffset();
    }, () =>
    {
        RenderSystem.PolygonOffset(0, 0);
        RenderSystem.DisablePolygonOffset();
    });
    
    public static LayeringStateShard ViewOffsetZLayering { get; } = new("view_offset_z_layering", () =>
    {
        var stack = RenderSystem.ModelViewStack;
        stack.PushPose();

        const float scale = 0.99975586f;
        stack.Scale(scale, scale, scale);
        RenderSystem.ApplyModelViewMatrix();
    }, () =>
    {
        var stack = RenderSystem.ModelViewStack;
        stack.PopPose();
        RenderSystem.ApplyModelViewMatrix();
    });

    public static OutputStateShard MainTarget { get; } = new("main_target", () => { }, () => { });
    
    public static OutputStateShard OutlineTarget { get; } = new("outline_target", () =>
    {
        Util.LogFoobar();
    }, () =>
    {
        Game.Instance.MainRenderTarget.BindWrite(false);
    });
    
    public static OutputStateShard TranslucentTarget { get; } = new("translucent_target", () => 
    {
        Util.LogFoobar();
    }, () => 
    { 
        Util.LogFoobar();
        Game.Instance.MainRenderTarget.BindWrite(false);
    });
    
    public static OutputStateShard ParticlesTarget { get; } = new("particles_target", () => 
    {
        Util.LogFoobar();
    }, () => 
    { 
        Util.LogFoobar();
        Game.Instance.MainRenderTarget.BindWrite(false);
    });
    
    public static OutputStateShard WeatherTarget { get; } = new("weather_target", () => 
    {
        Util.LogFoobar();
    }, () => 
    { 
        Util.LogFoobar();
        Game.Instance.MainRenderTarget.BindWrite(false);
    });
    
    public static OutputStateShard CloudsTarget { get; } = new("clouds_target", () => 
    {
        Util.LogFoobar();
    }, () => 
    { 
        Util.LogFoobar();
        Game.Instance.MainRenderTarget.BindWrite(false);
    });
    
    public static OutputStateShard ItemEntityTarget { get; } = new("item_entity_target", () => 
    {
        Util.LogFoobar();
    }, () =>
    { 
        Util.LogFoobar();
        Game.Instance.MainRenderTarget.BindWrite(false);
    });

    private readonly string _name;
    private readonly Action _setupState;
    private readonly Action _clearState;

    protected RenderStateShard(string name, Action setupState, Action clearState)
    {
        _name = name;
        _setupState = setupState;
        _clearState = clearState;
    }

    public void SetupRenderState() => _setupState();
    public void ClearRenderState() => _clearState();

    public override string ToString() => _name;

    public class ShaderStateShard : RenderStateShard
    {
        private readonly IOptional<Func<ShaderInstance?>> _shader;

        public ShaderStateShard(Func<ShaderInstance?> shader) : base("shader",
            () => RenderSystem.SetShader(shader),
            () => { })
        {
            _shader = Optional.Of(shader);
        }

        public ShaderStateShard() : base("shader",
            () => RenderSystem.SetShader(() => null),
            () => { })
        {
            _shader = Optional.Empty<Func<ShaderInstance>>();
        }
    }

    public class EmptyTextureStateShard : RenderStateShard
    {
        public EmptyTextureStateShard(Action setupState, Action clearState) : base("texture", setupState, clearState)
        {
        }

        public EmptyTextureStateShard() : this(() => { }, () => { })
        {
        }

        public virtual IOptional<ResourceLocation> CutoutTexture => Optional.Empty<ResourceLocation>();
    }

    public class TextureStateShard : EmptyTextureStateShard
    {
        private readonly IOptional<ResourceLocation> _location;
        private readonly bool _blur;
        private readonly bool _mipmap;

        public TextureStateShard(ResourceLocation location, bool blur, bool mipmap)
            : base(() =>
            {
                var textureManager = Game.Instance.TextureManager;
                textureManager.GetTexture(location).SetFilter(blur, mipmap);
                RenderSystem.SetShaderTexture(0, location);
            }, () => { })
        {
            _location = Optional.Of(location);
            _blur = blur;
            _mipmap = mipmap;
        }
    }

    public class MultiTextureStateShard : EmptyTextureStateShard
    {
        public override IOptional<ResourceLocation> CutoutTexture { get; }

        private MultiTextureStateShard(List<(ResourceLocation, bool, bool)> list)
            : base(() =>
            {
                var i = 0;
                var textureManager = Game.Instance.TextureManager;
                foreach (var (location, blur, mipmap) in list)
                {
                    textureManager.GetTexture(location).SetFilter(blur, mipmap);
                    RenderSystem.SetShaderTexture(i++, location);
                }
            }, () => { })
        {
            CutoutTexture = list
                .Select(Optional.Of)
                .FirstOrDefault(Optional.Empty<(ResourceLocation, bool, bool)>())
                .Select(x => x.Item1);
        }

        public static StateBuilder Builder => new();

        public class StateBuilder
        {
            private readonly List<(ResourceLocation, bool, bool)> _list = new();

            public StateBuilder Add(ResourceLocation location, bool blur, bool mipmap)
            {
                _list.Add((location, blur, mipmap));
                return this;
            }

            public MultiTextureStateShard Build() => new(_list);
        }
    }
    
    public class BoolStateShard : RenderStateShard
    {
        private readonly bool _enabled;
        
        public BoolStateShard(string name, Action setupState, Action clearState, bool enabled) 
            : base(name, setupState, clearState)
        {
            _enabled = enabled;
        }
    }

    public class TransparencyStateShard : RenderStateShard
    {
        public TransparencyStateShard(string name, Action setupState, Action clearState) : base(name, setupState,
            clearState)
        {
        }
    }
    
    public class LightmapStateShard : BoolStateShard
    {
        public LightmapStateShard(bool state)
            : base("lightmap", () =>
            {
                if (state) Game.Instance.GameRenderer.LightTexture.TurnOnLightLayer();
            }, () =>
            {
                if (state) Game.Instance.GameRenderer.LightTexture.TurnOffLightLayer();
            }, state) {}
    }
    
    public class OverlayStateShard : BoolStateShard
    {
        public OverlayStateShard(bool state)
            : base("overlay", () =>
            {
                if (state) Game.Instance.GameRenderer.OverlayTexture.SetupOverlayColor();
            }, () =>
            {
                if (state) Game.Instance.GameRenderer.OverlayTexture.TeardownOverlayColor();
            }, state) {}
    }

    public class CullStateShard : BoolStateShard
    {
        public CullStateShard(bool cull)
            : base("cull", () =>
            {
                if (!cull) RenderSystem.DisableCull();
            }, () =>
            {
                if (!cull) RenderSystem.EnableCull();
            }, cull) {}
    }

    public class DepthTestStateShard : RenderStateShard
    {
        private readonly string _funcName;

        public DepthTestStateShard(string name, ComparisonKind func)
            : base("depth_test", () =>
            {
                if (func != ComparisonKind.Always)
                {
                    RenderSystem.EnableDepthTest();
                    RenderSystem.DepthFunc(func);
                }
            }, () =>
            {
                if (func != ComparisonKind.Always)
                {
                    RenderSystem.DisableDepthTest();
                    RenderSystem.DepthFunc(func);
                }
            })
        {
            _funcName = name;
        }

        public override string ToString() => $"{_name}[{_funcName}]";
    }
    
    public class LayeringStateShard : RenderStateShard
    {
        public LayeringStateShard(string name, Action setupState, Action clearState) 
            : base(name, setupState, clearState)
        {
        }
    }
    
    public class OutputStateShard : RenderStateShard
    {
        public OutputStateShard(string name, Action setupState, Action clearState) 
            : base(name, setupState, clearState)
        {
        }
    }
}