using System.Runtime.InteropServices;

namespace MiaCrate.Client.Utils;

// ReSharper disable InconsistentNaming
public static class ExtraSDL
{
    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong SDL_GetTicks64();
}