using System.Runtime.CompilerServices;

namespace MiaCrate.Client.Oshi.MacOs;

public static class IOKitUtil
{
    public static IOService GetMatchingService(string serviceName)
    {
        var dict = IOKit.IOServiceMatching(serviceName);
        return dict.Handle == 0 ? new IOService() : GetMatchingService(dict.Dictionary);
    }

    public static IOIterator GetMatchingServices(string serviceName)
    {
        var dict = IOKit.IOServiceMatching(serviceName);
        return dict.Handle == 0 ? new IOIterator() : GetMatchingServices(dict.Dictionary);
    }

    public static IOService GetMatchingService(CFDictionaryRef dict)
    {
        IOKit.IOMasterPort(0, out var port);
        var service = IOKit.IOServiceGetMatchingService(port, dict);
        SystemB.MachPortDeallocate(SystemB.MachTaskSelf(), port);
        return service;
    }
    
    public static IOIterator GetMatchingServices(CFDictionaryRef dict)
    {
        IOKit.IOMasterPort(0, out var port);
        var result = IOKit.IOServiceGetMatchingServices(port, dict, out var iterator);
        SystemB.MachPortDeallocate(SystemB.MachTaskSelf(), port);
        return result == StdLibError.Ok ? iterator : new IOIterator();
    }
}