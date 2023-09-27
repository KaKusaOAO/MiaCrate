using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
    
    public static double Time => GLFW.GetTime();
}