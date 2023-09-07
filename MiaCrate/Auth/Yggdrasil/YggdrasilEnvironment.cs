using System.Runtime.CompilerServices;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Auth.Yggdrasil;

public class YggdrasilEnvironment
{
    private static readonly Dictionary<string, YggdrasilEnvironment> _values = new();

    public static readonly YggdrasilEnvironment Prod = new(
        "https://authserver.mojang.com",
        "https://api.mojang.com",
        "https://sessionserver.mojang.com",
        "https://api.minecraftservices.com");

    public static readonly YggdrasilEnvironment Staging = new(
        "https://authserver.mojang.com",
        "https://api.mojang.com",
        "https://sessionserver.mojang.com",
        "https://api.minecraftservices.com");
    
    public IEnvironment Environment { get; }

    public string Name => Environment.Name;
    
    private YggdrasilEnvironment(string authHost, string accountsHost, string sessionHost, string servicesHost, [CallerMemberName] string name = "")
    {
        _values.Add(name, this);
        Environment = IEnvironment.Create(authHost, accountsHost, sessionHost, servicesHost, name);
    }

    public static IOptional<IEnvironment> FromString(string? val)
    {
        return _values.Values
            .Where(e => val != null && val.ToLower() == e.Name)
            .FindFirst()
            .Select(x => x.Environment);
    }
}