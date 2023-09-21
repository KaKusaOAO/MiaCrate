using Mochi.Utils;

namespace MiaCrate.Client.Test;

internal static class Program
{
    public static void Main(string[] args)
    {
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();
        
    }
}