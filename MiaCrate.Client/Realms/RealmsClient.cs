namespace MiaCrate.Client.Realms;

public class RealmsClient
{
    private readonly string _sessionId;
    private readonly string _username;
    private readonly Game _game;
    private const string WorldsResourcePath = "worlds";
    private const string InvitesResourcePath = "invites";

    public static RealmsClient Create() => Create(Game.Instance);

    public static RealmsClient Create(Game game)
    {
        var name = game.User.Name;
        var sessionId = game.User.SessionId;
        return new RealmsClient(sessionId, name, game);
    }

    public RealmsClient(string sessionId, string username, Game game)
    {
        _sessionId = sessionId;
        _username = username;
        _game = game;
    }
}