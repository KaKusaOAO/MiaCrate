using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client;

public class ResourceLoadStateTracker
{
    private ReloadState? _reloadState;
    private int _reloadCount;
	
    public void StartReload(ReloadReason reloadReason, List<IPackResources> list)
    {
        ++_reloadCount;
        if (_reloadState != null && !_reloadState._finished)
        {
            Logger.Warn("Reload already ongoing, replacing");
        }

        _reloadState = new ReloadState(reloadReason, list.Select(p => p.PackId).ToList());
    }

    public void FinishReload()
    {
        if (_reloadState == null)
        {
            Logger.Warn("Trying to finish reload, but nothing was started");
            return;
        }

        _reloadState._finished = true;
    }
	
    public enum ReloadReason
    {
        Initial, Manual, Unknown
    }

    private class ReloadState
    {
        private readonly ReloadReason _reloadReason;
        private readonly List<string> _packs;
        public RecoveryInfo? _recoveryReloadInfo; 
        public bool _finished;

        public ReloadState(ReloadReason reloadReason, List<string> packs)
        {
            _reloadReason = reloadReason;
            _packs = packs;
        }
    }

    private class RecoveryInfo
    {
        private readonly Exception _exception;
    }
}