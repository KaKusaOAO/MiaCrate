using Mochi.Utils;

namespace MiaCrate.Data;

public static class DataGeneratorEntry
{
    public static void Main(string[] args)
    {
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();
        
        SharedConstants.TryDetectVersion();
    }
}