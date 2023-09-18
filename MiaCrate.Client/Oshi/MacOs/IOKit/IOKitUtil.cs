using System.Runtime.CompilerServices;

namespace MiaCrate.Client.Oshi.MacOs;

public static class IOKitUtil
{
    public static IOService GetMatchingService(string serviceName)
    {
        var dict = IOKit.IOServiceMatching(serviceName);
        return dict.Handle == 0 ? new IOService() : GetMatchingService(dict.Dictionary);
    }

    public static IOService GetMatchingService(CFDictionaryRef dict)
    {
        IOKit.IOMasterPort(0, out var port);
        var service = IOKit.IOServiceGetMatchingService(port, dict);
        SystemB.MachPortDeallocate(SystemB.MachTaskSelf(), port);
        return service;
    }
}