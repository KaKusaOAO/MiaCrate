using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Graphics;

public abstract class RenderStateShard
{
    public static readonly TransparencyStateShard NoTransparency = 
        new("no_transparency", RenderSystem.DisableBlend, () => { });

    public static readonly TransparencyStateShard AdditiveTransparency =
        new("additive_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static readonly TransparencyStateShard LightningTransparency =
        new("lightning_trasnparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });
    
    public static readonly TransparencyStateShard GlintTransparency =
        new("glint_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendingFactorSrc.SrcColor, BlendingFactorDest.One, 
                BlendingFactorSrc.Zero, BlendingFactorDest.One);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });
    
    public static readonly TransparencyStateShard CrumblingTransparency =
        new("crumbling_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor, 
                BlendingFactorSrc.One, BlendingFactorDest.Zero);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });
    
    public static readonly TransparencyStateShard TranslucentTransparency =
        new("translucent_transparency", () =>
        {
            RenderSystem.EnableBlend();
            RenderSystem.BlendFuncSeparate(
                BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, 
                BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }, () =>
        {
            RenderSystem.DisableBlend();
            RenderSystem.DefaultBlendFunc();
        });

    public static readonly ShaderStateShard NoShader = new();

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
        private readonly IOptional<Func<ShaderInstance>> _shader;

        public ShaderStateShard(Func<ShaderInstance> shader) : base("shader",
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
        
        public EmptyTextureStateShard() : this(() => { }, () => { }) {}

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

    public class TransparencyStateShard : RenderStateShard
    {
        public TransparencyStateShard(string name, Action setupState, Action clearState) : base(name, setupState, clearState)
        {
        }
    }

    public class DepthTestStateShard : RenderStateShard
    {
        private readonly string _funcName;

        public DepthTestStateShard(string name, DepthFunction func)
            : base("depth_test", () =>
            {
                if (func != DepthFunction.Always)
                {
                    RenderSystem.EnableDepthTest();
                    RenderSystem.DepthFunc(func);
                }
            }, () =>
            {
                if (func != DepthFunction.Always)
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
}