namespace MiaCrate.Auth;

public abstract class BaseAuthenticationService : IAuthenticationService
{
    public abstract IUserAuthentication CreateUserAuthentication(Agent agent);
    public abstract IMinecraftSessionService CreateMinecraftSessionService();
    public abstract IGameProfileRepository CreateProfileRepository();
}