using System.Net;
using Mochi.Utils;

namespace MiaCrate.Auth.Yggdrasil;

public class YggdrasilAuthenticationService : HttpAuthenticationService
{
    private readonly string _clientToken;
    private readonly IEnvironment _environment;

    public YggdrasilAuthenticationService(IWebProxy proxy, IEnvironment env)
        : this(proxy, null, env)
    {
        
    }

    public YggdrasilAuthenticationService(IWebProxy proxy, string? clientToken = null) 
        : this(proxy, clientToken, YggdrasilEnvironment.Prod.Environment)
    {
        
    }

    public YggdrasilAuthenticationService(IWebProxy proxy, string? clientToken, IEnvironment env) 
        : base(proxy)
    {
        _clientToken = clientToken;
        _environment = env;
        Logger.Info($"Environment: {_environment.AsString()}");
    }

    public override IUserAuthentication CreateUserAuthentication(Agent agent)
    {
        throw new NotImplementedException();
    }

    public override IMinecraftSessionService CreateMinecraftSessionService()
    {
        throw new NotImplementedException();
    }

    public override IGameProfileRepository CreateProfileRepository()
    {
        throw new NotImplementedException();
    }
}