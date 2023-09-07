namespace MiaCrate.Auth;

public interface IUserAuthentication
{
    public bool CanLogin { get; }
    public void Login();
    public void Logout();
    public bool IsLoggedIn { get; }
    public bool CanPlayOnline { get; }
    public List<GameProfile> AvailableProfiles { get; }
    public GameProfile SelectedProfile { get; }
    public void SelectGameProfile(GameProfile profile);
}