using OpenTK.Audio.OpenAL;

namespace MiaCrate.Client.Audio;

public class Library
{
    private ALDevice _currentDevice;

    public Library()
    {
        
    }

    public void Init(string? str, bool bl)
    {
        _currentDevice = OpenDeviceOrFallback(str);
    }

    private static ALDevice OpenDeviceOrFallback(string? str)
    {
        ALDevice? device = null;
        
        if (str != null)
        {
            device = TryOpenDevice(str);
        }

        device ??= TryOpenDevice(DefaultDeviceName) ?? TryOpenDevice(null);

        if (!device.HasValue)
            throw new Exception("Failed to open OpenAL device");

        return device.Value;
    }

    public static string? DefaultDeviceName
    {
        get
        {
            if (!ALC.IsExtensionPresent(ALDevice.Null, "ALC_ENUMERATE_ALL_EXT")) return null;
            ALC.GetString(ALDevice.Null, AlcGetStringList.AllDevicesSpecifier);
            return ALC.GetString(ALDevice.Null, AlcGetString.DefaultAllDevicesSpecifier);
        }
    }


    private static bool TryOpenDevice(string? str, out ALDevice device)
    {
        device = ALC.OpenDevice(str);
        return device != ALDevice.Null && !OpenAlUtil.CheckAlcError(device, "Open device");
    }

    private static ALDevice? TryOpenDevice(string? str) => 
        TryOpenDevice(str, out var result) ? result : null;
}