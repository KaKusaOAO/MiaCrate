using System.Text;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate;

public class FilledProfileResults : IProfileResults
{
    private static readonly IProfilerPathEntry _empty = new EmptyEntry();
    private readonly Dictionary<string, IProfilerPathEntry> _entries;
    private readonly int _tickDuration;

    public long StartTimeNano { get; }

    public int StartTimeTicks { get; }

    public long EndTimeNano { get; }

    public int EndTimeTicks { get; }

    private IProfileResults Boxed => this;

    private Dictionary<string, CounterCollector> CounterValues
    {
        get
        {
            var dict = new Dictionary<string, CounterCollector>();
            foreach (var (key, value) in _entries)
            {
                var map = value.Counters;
                if (!map.Any()) continue;
                
                var list = key.Split(IProfileResults.PathSeparator).ToList();
                foreach (var (s, l) in map)
                {
                    dict.ComputeIfAbsent(s, _ => new CounterCollector())
                        .AddValue(list.GetEnumerator(), l);
                }
            }

            return dict;
        }
    }

    public FilledProfileResults(Dictionary<string, IProfilerPathEntry> entries, long startTimeNano, int startTimeTicks, long endTimeNano, int endTimeTicks)
    {
        _entries = entries;
        StartTimeNano = startTimeNano;
        StartTimeTicks = startTimeTicks;
        EndTimeNano = endTimeNano;
        EndTimeTicks = endTimeTicks;
        _tickDuration = endTimeTicks - startTimeTicks;
    }

    private IProfilerPathEntry GetEntry(string str) => _entries.GetValueOrDefault(str, _empty);

    public List<ResultField> GetTimes(string str)
    {
        var entry = GetEntry("root");
        var l = entry.Duration;

        var entry2 = GetEntry(str);
        var m = entry2.Duration;
        var n = entry2.Count;

        var list = new List<ResultField>();
        if (!string.IsNullOrEmpty(str))
        {
            str += IProfileResults.PathSeparator;
        }

        var o = 0L;
        foreach (var key in _entries.Keys)
        {
            if (IsDirectChild(str, key))
            {
                o += GetEntry(key).Duration;
            }
        }

        var f = (float) o;
        if (o < m) o = m;
        if (l < o) l = o;
        
        foreach (var key in _entries.Keys)
        {
            if (IsDirectChild(str, key))
            {
                var entry3 = GetEntry(key);
                var p = entry3.Duration;
                var d = p * 100.0 / o;
                var e = p * 100.0 / l;

                var str5 = key[str.Length..];
                list.Add(new ResultField(str5, d, e, entry.Count));
            }
        }

        if (o > f)
        {
            list.Add(new ResultField("unspecified", (o - f) * 100.0 / o, (o - f) * 100.0 / l, n));
        }
        
        list.Sort();
        list.Insert(0, new ResultField(str, 100.0, o * 100.0 / l, n));
        return list;
    }

    private static bool IsDirectChild(string a, string b) => 
        b.Length > a.Length && b.StartsWith(a) &&
        b.IndexOf(IProfileResults.PathSeparator, a.Length + 1) < 0;

    public bool SaveResults(string path)
    {
        try
        {
            var dirName = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dirName);

            using var stream = new FileStream(path, FileMode.OpenOrCreate);
            using var writer = new StreamWriter(stream);
            writer.Write(GetProfilerResults(Boxed.NanoDuration, Boxed.TickDuration));
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Could not save profiler results to {path}");
            Logger.Error(ex);
            return false;
        }
    }

    private string GetProfilerResults(long l, int i)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---- {MiaCore.ProductName} Profiler Results ----");
        sb.Append("// ").Append(GetComment());
        sb.AppendLine();
        sb.AppendLine();

        sb.Append("Version: ").AppendLine(SharedConstants.CurrentVersion.Id);
        sb.Append("Time span: ").Append(l / 1_000_000L).AppendLine(" ms");
        sb.Append("Tick span: ").Append(i).AppendLine(" ticks");

        var ticksFloat = i / (l / 1.0e9f);
        sb.Append($"// This is approximately ").Append($"{ticksFloat:F2}").Append(" ticks per second. It should be ")
            .Append(SharedConstants.TicksPerSecond).AppendLine(" ticks per second");
        sb.AppendLine();
        
        // TODO: Dump profile

        return sb.ToString();
    }

    private static string GetComment()
    {
        var arr = new[]
        {
            "I'd Rather Be Surfing", "Shiny numbers!", "Am I not running fast enough? :(",
            "I'm working as hard as I can!", "Will I ever be good enough for you? :(", "Speedy. Zoooooom!",
            "Hello world", "40% better than a crash report.", "Now with extra numbers", "Now with less numbers",
            "Now with the same numbers", "You should add flames to things, it makes them go faster!",
            "Do you feel the need for... optimization?", "*cracks redstone whip*",
            "Maybe if you treated it better then it'll have more motivation to work faster! Poor server."
        };

        try
        {
            return arr[Util.GetNanos() % arr.Length];
        }
        catch
        {
            return "Witty comment unavailable :(";
        }
    }

    private class CounterCollector
    {
        public long SelfValue { get; set; }
        public long TotalValue { get; set; }
        public Dictionary<string, CounterCollector> Children { get; } = new();

        public void AddValue(IEnumerator<string> source, long l)
        {
            TotalValue += l;
            if (!source.MoveNext())
            {
                SelfValue += l;
                return;
            }

            Children.ComputeIfAbsent(source.Current, _ => new CounterCollector())
                .AddValue(source, l);
        }
    }
    
    private class EmptyEntry : IProfilerPathEntry
    {
        public long Duration => 0;

        public long MaxDuration => 0;

        public long Count => 0;

        public Dictionary<string, long> Counters { get; } = new();
    }
}