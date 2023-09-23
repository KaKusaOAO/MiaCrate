using MiaCrate.Client.Systems;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Shaders;

public class BlendMode
{
    private static BlendMode? _lastApplied;
    
    private readonly bool _separateBlend;
    private readonly BlendingFactorSrc _srcRgb;
    private readonly BlendingFactorDest _dstRgb;
    private readonly BlendingFactorSrc _srcAlpha;
    private readonly BlendingFactorDest _dstAlpha;
    private readonly BlendEquationMode _blendFunc;

    public bool IsOpaque { get; }
    
    private BlendMode(bool separateBlend, bool opaque, 
        BlendingFactorSrc srcRgb, BlendingFactorDest dstRgb,
        BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha, BlendEquationMode blendFunc)
    {
        _separateBlend = separateBlend;
        IsOpaque = opaque;
        _srcRgb = srcRgb;
        _dstRgb = dstRgb;
        _srcAlpha = srcAlpha;
        _dstAlpha = dstAlpha;
        _blendFunc = blendFunc;
    }
    
    public BlendMode() : this(false, true, 
        BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One,
        BlendingFactorDest.Zero, BlendEquationMode.FuncAdd)
    {
    }

    public BlendMode(BlendingFactorSrc sourceFactor, BlendingFactorDest destFactor, BlendEquationMode blendFunc)
        : this(false, false, sourceFactor, destFactor, sourceFactor, destFactor, blendFunc)
    {
        
    }

    public BlendMode(BlendingFactorSrc srcRgb, BlendingFactorDest dstRgb,
        BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha, BlendEquationMode blendFunc)
        : this(true, false, srcRgb, dstRgb, srcAlpha, dstAlpha, blendFunc)
    {
        
    }

    public void Apply()
    {
        if (this == _lastApplied) return;
        if (_lastApplied == null || IsOpaque != _lastApplied.IsOpaque)
        {
            _lastApplied = this;
            if (IsOpaque)
            {
                RenderSystem.DisableBlend();
                return;
            }
            
            RenderSystem.EnableBlend();
        }

        RenderSystem.BlendEquation(_blendFunc);
        if (_separateBlend)
        {
            RenderSystem.BlendFuncSeparate(_srcRgb, _dstRgb, _srcAlpha, _dstAlpha);
        }
        else
        {
            RenderSystem.BlendFunc(_srcRgb, _dstRgb);
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BlendMode other) return false;
        if (ReferenceEquals(this, other)) return true;

        if (_separateBlend != other._separateBlend) return false;
        if (_srcRgb != other._srcRgb) return false;
        if (_srcAlpha != other._srcAlpha) return false;
        if (_dstRgb != other._dstRgb) return false;
        if (_dstAlpha != other._dstAlpha) return false;
        if (IsOpaque != other.IsOpaque) return false;
        return _blendFunc == other._blendFunc;
    }

    public override int GetHashCode() => 
        HashCode.Combine(_srcRgb, _srcAlpha, _dstRgb, _dstAlpha, _blendFunc, _separateBlend, IsOpaque);

    public static bool operator ==(BlendMode? a, BlendMode? b) => a?.Equals(b) ?? ReferenceEquals(b, null);
    public static bool operator !=(BlendMode? a, BlendMode? b) => !(a?.Equals(b) ?? !ReferenceEquals(b, null));

    public static BlendEquationMode StringToBlendFunc(string str)
    {
        str = str.Trim().ToLowerInvariant();
        
        return str switch
        {
            "add" => BlendEquationMode.FuncAdd,
            "subtract" => BlendEquationMode.FuncSubtract,
            "reversesubtract" or "reverse_subtract" => BlendEquationMode.FuncReverseSubtract,
            "min" => BlendEquationMode.Min,
            "max" => BlendEquationMode.Max,
            _ => BlendEquationMode.FuncAdd
        };
    }

    public static BlendingFactor StringToBlendFactor(string str)
    {
        str = str.Trim().ToLowerInvariant()
            .Replace("_", "")
            .Replace("one", "1")
            .Replace("zero", "0")
            .Replace("minus", "-");

        return str switch
        {
            "0" => BlendingFactor.Zero,
            "1" => BlendingFactor.One,
            "srccolor" => BlendingFactor.SrcColor,
            "1-srccolor" => BlendingFactor.OneMinusSrcColor,
            "dstcolor" => BlendingFactor.DstColor,
            "1-dstcolor" => BlendingFactor.OneMinusDstColor,
            "srcalpha" => BlendingFactor.SrcAlpha,
            "1-srcalpha" => BlendingFactor.OneMinusSrcAlpha,
            "dstalpha" => BlendingFactor.DstAlpha,
            "1-dstalpha" => BlendingFactor.OneMinusDstAlpha,
            _ => (BlendingFactor) (-1)
        };
    }

    public static BlendingFactorSrc StringToBlendFactorSrc(string str) => (BlendingFactorSrc) StringToBlendFactor(str);
    public static BlendingFactorDest StringToBlendFactorDest(string str) => (BlendingFactorDest) StringToBlendFactor(str);
}