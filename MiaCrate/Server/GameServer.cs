using MiaCrate.Commands;
using Mochi.Texts;

namespace MiaCrate.Server;

public class GameServer : ReentrantBlockableEventLoop<TickTask>, ICommandSource, IDisposable
{
    public GameServer() : base("Server")
    {
        Util.LogFoobar();
    }

    protected override Thread RunningThread => throw new NotImplementedException();

    protected override TickTask WrapRunnable(IRunnable runnable)
    {
        throw new NotImplementedException();
    }

    protected override bool ShouldRun(TickTask runnable)
    {
        throw new NotImplementedException();
    }

    public void SendSystemMessage(IComponent component)
    {
        throw new NotImplementedException();
    }

    public bool AcceptsSuccess => throw new NotImplementedException();

    public bool AcceptsFailure => throw new NotImplementedException();

    public bool ShouldInformAdmins => throw new NotImplementedException();
}