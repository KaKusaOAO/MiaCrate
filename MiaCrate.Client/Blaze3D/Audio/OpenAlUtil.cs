using Mochi.Utils;
using OpenTK.Audio.OpenAL;

namespace MiaCrate.Client.Audio;

public static class OpenAlUtil
{
    public static bool CheckAlError(string str)
    {
        var error = AL.GetError();
        if (error == ALError.NoError) return false;
        
        Logger.Error($"{str}: {AlErrorToString(error)}");
        return true;
    }

    private static string AlErrorToString(ALError error)
    {
        return error switch
        {
            ALError.InvalidName => "Invalid name parameter.",
            ALError.InvalidEnum => "Invalid enumerated parameter value.",
            ALError.InvalidValue => "Invalid parameter parameter value.",
            ALError.InvalidOperation => "Invalid operation.",
            ALError.OutOfMemory => "Unable to allocate memory.",
            _ => "An unrecognized error occurred."
        };
    }

    public static bool CheckAlcError(ALDevice device, string str)
    {
        var error = ALC.GetError(device);
        if (error == AlcError.NoError) return false;
        
        Logger.Error($"{str}{device}: {AlcErrorToString(error)}");
        return true;
    }

    private static string AlcErrorToString(AlcError error)
    {
        return error switch
        {
            AlcError.InvalidDevice => "Invalid device.",
            AlcError.InvalidContext => "Invalid context.",
            AlcError.InvalidEnum => "Invalid enum.",
            AlcError.InvalidValue => "Invalid value.",
            AlcError.OutOfMemory => "Unable to allocate memory.",
            _ => "An unrecognized error occurred."
        };
    }
}