using System.Net;

namespace MiaCrate.Auth;

public interface IMinecraftSessionService
{
    public void JoinServer(GameProfile profile, string authToken, string serverId);
    public GameProfile HasJoinedServer(GameProfile profile, string serverId, IPAddress address);
    public Dictionary<MinecraftProfileTexture.TextureType, MinecraftProfileTexture> GetTextures(GameProfile profile,
        bool requireSecure);
    public GameProfile FillProfileProperties(GameProfile profile, bool requireSecure);
    public string GetSecurePropertyValue(Property property);
}