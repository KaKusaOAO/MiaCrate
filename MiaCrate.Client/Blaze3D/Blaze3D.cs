using MiaCrate.Client.Utils;
using SDL2;

namespace MiaCrate.Client;

public static class Blaze3D
{
    public static void YouJustLostTheGame()
    {
        unsafe
        {
            *(byte*) 0 = 1;
        }
    }

    public static double Time => SDL.SDL_GetTicks() / 1000.0;
}