namespace MiaCrate.Auth;

public interface IAuthenticationService
{
    public IUserAuthentication CreateUserAuthentication(Agent agent);
    public IMinecraftSessionService CreateMinecraftSessionService();
    public IGameProfileRepository CreateProfileRepository();
}