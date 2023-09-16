using Mochi.Utils;
using OpenTK.Audio.OpenAL;

namespace MiaCrate.Client.Audio;

public static class OpenAlUtil
{
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