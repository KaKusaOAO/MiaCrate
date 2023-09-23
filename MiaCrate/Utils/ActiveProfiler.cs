using MiaCrate.Extensions;
using Microsoft.VisualBasic.CompilerServices;
using Mochi.Utils;

namespace MiaCrate;

public class ActiveProfiler : IProfileCollector
{
    // 100ms = 100 * 1,000,000ns
    private const long WarningTimeNanos = 100 * 1_000_000L;
    
    private readonly Dictionary<string, PathEntry> _entries = new();
    private readonly List<string> _paths = new();
    private readonly List<long> _startTimes = new();
    private readonly long _startTimeNano;
    private readonly Func<long> _getRealTime;
    private readonly int _startTimeTicks;
    private readonly Func<int> _getTickTime;
    private readonly bool _warn;
    private bool _started;
    private string _path = "";
    private PathEntry? _currentEntry;

    private PathEntry CurrentEntry => _currentEntry ??= _entries.ComputeIfAbsent(_path, _ => new PathEntry());

    public ActiveProfiler(Func<long> getRealTime, Func<int> getTickTime, bool warn)
    {
        _startTimeNano = getRealTime();
        _getRealTime = getRealTime;
        _startTimeTicks = getTickTime();
        _getTickTime = getTickTime;
        _warn = warn;
    }
    
    public void StartTick()
    {
        if (_started)
        {
            Logger.Error("Profiler tick already started - missing EndTick()?");
            return;
        }

        _started = true;
        _path = "";
        _paths.Clear();
        Push("root");
    }

    public void EndTick()
    { 
        if (!_started)
        {
            Logger.Error("Profiler tick already ended - missing StartTick()?");
            return;
        }

        Pop();
        _started = false;
        if (_paths.Any())
        {
            Logger.Error("Profiler tick ended before path was fully popped (remainder: '"
                         + LogUtils.Defer(() => IProfileResults.DemanglePath(_path))
                         + "'). Mismatched push/pop?");
        }
    }

    public void Push(string str)
    {
        if (!_started)
        {
            Logger.Error($"Cannot push '{str}' to profiler if profiler tick hasn't started - missing StartTick()?");
            return;
        }

        if (_paths.Any())
        {
            _path += IProfileResults.PathSeparator;
        }

        _path += str;
        _paths.Add(_path);
        _startTimes.Add(Util.GetNanos()); // ?
        _currentEntry = null;
    }

    public void Push(Func<string> func) => Push(func());

    public void Pop()
    {
        if (!_started)
        {
            Logger.Error($"Cannot pop from profiler if profiler tick hasn't started - missing StartTick()?");
            return;
        }

        if (!_startTimes.Any())
        {
            Logger.Error("Tried to pop one too many times! Mismatched Push() and Pop()?");
            return;
        }

        var l = Util.GetNanos();
        var m = _startTimes[^1];
        _startTimes.RemoveAt(_startTimes.Count - 1);
        _paths.RemoveAt(_paths.Count - 1);

        var n = l - m;
        var entry = CurrentEntry;
        entry.AccumulatedDuration += n;
        entry.Count++;
        entry.MaxDuration = Math.Max(entry.MaxDuration, n);
        entry.MinDuration = Math.Max(entry.MinDuration, n);
        if (_warn && n > WarningTimeNanos)
        {
            Logger.Warn("Something's taking too long! '"
                + LogUtils.Defer(() => IProfileResults.DemanglePath(_path))
                + "' took approx "
                + LogUtils.Defer(() => n / 1_000_000.0)
                + " ms");
        }

        _path = !_paths.Any() ? "" : _paths[^1];
        _currentEntry = null;
    }

    public void PopPush(string str)
    {
        Pop();
        Push(str);
    }

    public void PopPush(Func<string> func)
    {
        Pop();
        Push(func);
    }

    public void IncrementCounter(Func<string> func, int count = 1)
    {
        var key = func();
        var v = CurrentEntry.Counters.ComputeIfAbsent(key, _ => 0);
        CurrentEntry.Counters[key] = v + count;
    }

    public void IncrementCounter(string str, int count = 1)
    {
        var v = CurrentEntry.Counters.ComputeIfAbsent(str, _ => 0);
        CurrentEntry.Counters[str] = v + count;
    }

    public IProfileResults Results =>
        new FilledProfileResults(
            _entries.ToDictionary(e => e.Key, e => (IProfilerPathEntry) e.Value), 
            _startTimeNano, _startTimeTicks, _getRealTime(), _getTickTime());

    public PathEntry? GetEntry(string str) => _entries.GetValueOrDefault(str);

    public class PathEntry : IProfilerPathEntry
    {
        public long MaxDuration { get; set; } = long.MinValue;
        public long MinDuration { get; set; } = long.MaxValue;
        public long AccumulatedDuration { get; set; }
        public long Count { get; set; }
        public Dictionary<string, long> Counters { get; } = new();
        
        long IProfilerPathEntry.Duration => AccumulatedDuration;
    }
}