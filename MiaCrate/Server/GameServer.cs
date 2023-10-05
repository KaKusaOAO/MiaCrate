using MiaCrate.Commands;
using MiaCrate.Common;
using MiaCrate.Core;
using Mochi.Texts;

namespace MiaCrate.Server;

public class GameServer : ReentrantBlockableEventLoop<TickTask>, ICommandSource, IDisposable
{
    public IRegistryAccess.IFrozen RegistryAccess { get; }
    
    public IProfilerFiller Profiler { get; }

    public virtual int MaxChainedNeighborUpdates => SharedConstants.MaxChainedNeighborUpdates;
    
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