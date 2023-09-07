namespace MiaCrate.Auth;

public interface IGameProfileRepository
{
    public void FindProfilesByNames(List<string> names, Agent agent, IProfileLookupCallback callback);
}