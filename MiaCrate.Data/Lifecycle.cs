namespace MiaCrate.Data;

public class Lifecycle
{

    public static Lifecycle Stable { get; } = new();

    public static Lifecycle Experimental { get; } = new();

    public static Lifecycle Deprecated(int since) => new DeprecatedLifecycle(since);
    
    private Lifecycle()
    {
        
    }

    public static Lifecycle operator +(Lifecycle a, Lifecycle b)
    {
        if (a == Experimental || b == Experimental) return Experimental;
        if (a is DeprecatedLifecycle self)
        {
            if (b is DeprecatedLifecycle other && other.Since < self.Since) return other;
            return a;
        }

        return b is DeprecatedLifecycle ? b : Stable;
    }

    public override string? ToString()
    {
        if (this == Stable) return "Stable";
        if (this == Experimental) return "Experimental";
        return base.ToString();
    }

    public class DeprecatedLifecycle : Lifecycle
    {
        public int Since { get; }

        public DeprecatedLifecycle(int since)
        {
            Since = since;
        }
    }
}