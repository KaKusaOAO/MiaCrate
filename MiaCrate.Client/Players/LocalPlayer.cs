using MiaCrate.Client.Multiplayer;
using MiaCrate.Statistics;

namespace MiaCrate.Client.Players;

public class LocalPlayer : AbstractClientPlayer
{
    public LocalPlayer(Game game, ClientLevel level, ClientPacketListener connection, StatsCounter stats,
        ClientRecipeBook recipeBook, bool wasShiftLeyDOwn, bool wasSprinting)
        : base(level, connection.LocalGameProfile)
    {
        throw new NotImplementedException();
    }
}