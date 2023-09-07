namespace MiaCrate.Auth;

public interface IProfileLookupCallback
{
    public void OnProfileLookupSucceed(GameProfile profile);
    public void OnProfileLookupFailed(GameProfile profile, Exception exception);
}