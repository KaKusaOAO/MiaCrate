using MiaCrate.Client.Systems;
using Veldrid;

namespace MiaCrate.Client.Shaders;

public class BlendMode
{
    private static BlendMode? _lastApplied;
    
    private readonly bool _separateBlend;
    private readonly BlendFactor _srcRgb;
    private readonly BlendFactor _dstRgb;
    private readonly BlendFactor _srcAlpha;
    private readonly BlendFactor _dstAlpha;
    private readonly BlendFunction _blendFunc;

    public bool IsOpaque { get; }
    
    private BlendMode(bool separateBlend, bool opaque, 
        BlendFactor srcRgb, BlendFactor dstRgb,
        BlendFactor srcAlpha, BlendFactor dstAlpha, BlendFunction blendFunc)
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
        BlendFactor.One, BlendFactor.Zero, BlendFactor.One,
        BlendFactor.Zero, BlendFunction.Add)
    {
    }

    public BlendMode(BlendFactor sourceFactor, BlendFactor destFactor, BlendFunction blendFunc)
        : this(false, false, sourceFactor, destFactor, sourceFactor, destFactor, blendFunc)
    {
        
    }

    public BlendMode(BlendFactor srcRgb, BlendFactor dstRgb,
        BlendFactor srcAlpha, BlendFactor dstAlpha, BlendFunction blendFunc)
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

    public static BlendFunction StringToBlendFunc(string str)
    {
        str = str.Trim().ToLowerInvariant();
        
        return str switch
        {
            "add" => BlendFunction.Add,
            "subtract" => BlendFunction.Subtract,
            "reversesubtract" or "reverse_subtract" => BlendFunction.ReverseSubtract,
            "min" => BlendFunction.Minimum,
            "max" => BlendFunction.Maximum,
            _ => BlendFunction.Add
        };
    }

    public static BlendFactor StringToBlendFactor(string str)
    {
        str = str.Trim().ToLowerInvariant()
            .Replace("_", "")
            .Replace("one", "1")
            .Replace("zero", "0")
            .Replace("minus", "-");

        return str switch
        {
            "0" => BlendFactor.Zero,
            "1" => BlendFactor.One,
            "srccolor" => BlendFactor.SourceColor,
            "1-srccolor" => BlendFactor.InverseSourceColor,
            "dstcolor" => BlendFactor.DestinationColor,
            "1-dstcolor" => BlendFactor.InverseDestinationColor,
            "srcalpha" => BlendFactor.SourceAlpha,
            "1-srcalpha" => BlendFactor.InverseSourceAlpha,
            "dstalpha" => BlendFactor.DestinationAlpha,
            "1-dstalpha" => BlendFactor.InverseSourceAlpha,
            _ => throw new ArgumentOutOfRangeException(nameof(str), str, null)
        };
    }

    public static BlendFactor StringToBlendFactorSrc(string str) => StringToBlendFactor(str);
    public static BlendFactor StringToBlendFactorDest(string str) => StringToBlendFactor(str);
}