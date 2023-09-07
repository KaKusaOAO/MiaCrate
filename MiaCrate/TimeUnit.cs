namespace MiaCrate;

public sealed class TimeUnit
{
    /**
     * Time unit representing one thousandth of a microsecond.
     */
    public static readonly TimeUnit Nanoseconds = new(NanoScale);

    /**
     * Time unit representing one thousandth of a millisecond.
     */
    public static readonly TimeUnit Microseconds = new(MicroScale);

    /**
     * Time unit representing one thousandth of a second.
     */
    public static readonly TimeUnit Milliseconds = new(MilliScale);

    /**
     * Time unit representing one second.
     */
    public static readonly TimeUnit Seconds = new(SecondScale);

    /**
     * Time unit representing sixty seconds.
     * @since 1.6
     */
    public static readonly TimeUnit Minutes = new(MinuteScale);

    /**
     * Time unit representing sixty minutes.
     * @since 1.6
     */
    public static readonly TimeUnit Hours = new(HourScale);

    /**
     * Time unit representing twenty four hours.
     * @since 1.6
     */
    public static readonly TimeUnit Days = new(DayScale);

    // Scales as constants
    private const long NanoScale = 1L;
    private const long MicroScale = 1000L * NanoScale;
    private const long MilliScale = 1000L * MicroScale;
    private const long SecondScale = 1000L * MilliScale;
    private const long MinuteScale = 60L * SecondScale;
    private const long HourScale = 60L * MinuteScale;
    private const long DayScale = 24L * HourScale;

    /*
     * Instances cache conversion ratios and saturation cutoffs for
     * the units up through SECONDS. Other cases compute them, in
     * method cvt.
     */

    private readonly long _scale;
    private readonly long _maxNanos;
    private readonly long _maxMicros;
    private readonly long _maxMillis;
    private readonly long _maxSecs;
    private readonly long _microRatio;
    private readonly int _milliRatio; // fits in 32 bits
    private readonly int _secRatio; // fits in 32 bits

    private TimeUnit(long s)
    {
        _scale = s;
        _maxNanos = long.MaxValue / s;
        var ur = (s >= MicroScale) ? (s / MicroScale) : (MicroScale / s);
        _microRatio = ur;
        _maxMicros = long.MaxValue / ur;
        var mr = (s >= MilliScale) ? (s / MilliScale) : (MilliScale / s);
        _milliRatio = (int) mr;
        _maxMillis = long.MaxValue / mr;
        var sr = (s >= SecondScale) ? (s / SecondScale) : (SecondScale / s);
        _secRatio = (int) sr;
        _maxSecs = long.MaxValue / sr;
    }

    /**
     * General conversion utility.
     *
     * @param d duration
     * @param dst result unit scale
     * @param src source unit scale
     */
    private static long Cvt(long d, long dst, long src)
    {
        long r, m;
        if (src == dst)
            return d;
        if (src < dst)
            return d / (dst / src);
        if (d > (m = long.MaxValue / (r = src / dst)))
            return long.MaxValue;
        if (d < -m)
            return long.MinValue;
        return d * r;
    }

    /**
     * Converts the given time duration in the given unit to this unit.
     * Conversions from finer to coarser granularities truncate, so
     * lose precision. For example, converting {@code 999} milliseconds
     * to seconds results in {@code 0}. Conversions from coarser to
     * finer granularities with arguments that would numerically
     * overflow saturate to {@code long.MinValue} if negative or
     * {@code long.MaxValue} if positive.
     *
     * <p>For example, to convert 10 minutes to milliseconds, use:
     * {@code TimeUnit.MILLISECONDS.convert(10L, TimeUnit.MINUTES)}
     *
     * @param sourceDuration the time duration in the given {@code sourceUnit}
     * @param sourceUnit the unit of the {@code sourceDuration} argument
     * @return the converted duration in this unit,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     */
    public long Convert(long sourceDuration, TimeUnit sourceUnit)
    {
        if (this == Nanoseconds)
            return sourceUnit.ToNanos(sourceDuration);
        if (this == Microseconds)
            return sourceUnit.ToMicros(sourceDuration);
        if (this == Milliseconds)
            return sourceUnit.ToMillis(sourceDuration);
        if (this == Seconds)
            return sourceUnit.ToSeconds(sourceDuration);
        return Cvt(sourceDuration, _scale, sourceUnit._scale);
    }

    /**
     * Converts the given time duration to this unit.
     *
     * <p>For any TimeUnit {@code unit},
     * {@code unit.convert(Duration.ofNanos(n))}
     * is equivalent to
     * {@code unit.convert(n, NANOSECONDS)}, and
     * {@code unit.convert(Duration.of(n, unit.toChronoUnit()))}
     * is equivalent to {@code n} (in the absence of overflow).
     *
     * @apiNote
     * This method differs from {@link Duration#toNanos()} in that it
     * does not throw {@link ArithmeticException} on numeric overflow.
     *
     * @param duration the time duration
     * @return the converted duration in this unit,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     * @throws NullPointerException if {@code duration} is null
     * @see Duration#of(long,TemporalUnit)
     * @since 11
     */
    public long Convert(TimeSpan duration)
    {
        long secs = (long) Math.Floor(duration.TotalSeconds);
        long nano = duration.Ticks * 100;
        if (secs < 0 && nano > 0)
        {
            // use representation compatible with integer division
            secs++;
            nano -= (int) SecondScale;
        }

        long s, nanoVal;
        // Optimize for the common case - NANOSECONDS without overflow
        if (this == Nanoseconds)
            nanoVal = nano;
        else if ((s = _scale) < SecondScale)
            nanoVal = nano / s;
        else if (this == Seconds)
            return secs;
        else
            return secs / _secRatio;
        long val = secs * _secRatio + nanoVal;
        return ((secs < _maxSecs && secs > -_maxSecs) ||
                (secs == _maxSecs && val > 0) ||
                (secs == -_maxSecs && val < 0))
            ? val
            : (secs > 0)
                ? long.MaxValue
                : long.MinValue;
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) NANOSECONDS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     */
    public long ToNanos(long duration)
    {
        long s, m;
        if ((s = _scale) == NanoScale)
            return duration;
        if (duration > (m = _maxNanos))
            return long.MaxValue;
        if (duration < -m)
            return long.MinValue;
        return duration * s;
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) MICROSECONDS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     */
    public long ToMicros(long duration)
    {
        long s, m;
        if ((s = _scale) <= MicroScale)
            return (s == MicroScale) ? duration : duration / _microRatio;
        if (duration > (m = _maxMicros))
            return long.MaxValue;
        if (duration < -m)
            return long.MinValue;
        return duration * _microRatio;
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) MILLISECONDS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     */
    public long ToMillis(long duration)
    {
        long s, m;
        if ((s = _scale) <= MilliScale)
            return (s == MilliScale) ? duration : duration / _milliRatio;
        if (duration > (m = _maxMillis))
            return long.MaxValue;
        if (duration < -m)
            return long.MinValue;
        return duration * _milliRatio;
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) SECONDS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     */
    public long ToSeconds(long duration)
    {
        long s, m;
        if ((s = _scale) <= SecondScale)
            return (s == SecondScale) ? duration : duration / _secRatio;
        if (duration > (m = _maxSecs))
            return long.MaxValue;
        if (duration < -m)
            return long.MinValue;
        return duration * _secRatio;
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) MINUTES.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     * @since 1.6
     */
    public long ToMinutes(long duration)
    {
        return Cvt(duration, MinuteScale, _scale);
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) HOURS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration,
     * or {@code long.MinValue} if conversion would negatively overflow,
     * or {@code long.MaxValue} if it would positively overflow.
     * @since 1.6
     */
    public long ToHours(long duration)
    {
        return Cvt(duration, HourScale, _scale);
    }

    /**
     * Equivalent to
     * {@link #convert(long, TimeUnit) DAYS.convert(duration, this)}.
     * @param duration the duration
     * @return the converted duration
     * @since 1.6
     */
    public long ToDays(long duration)
    {
        return Cvt(duration, DayScale, _scale);
    }

    /**
     * Utility to compute the excess-nanosecond argument to wait,
     * sleep, join.
     * @param d the duration
     * @param m the number of milliseconds
     * @return the number of nanoseconds
     */
    private int ExcessNanos(long d, long m)
    {
        long s;
        if ((s = _scale) == NanoScale)
            return (int) (d - m * MilliScale);
        if (s == MicroScale)
            return (int) (d * 1000L - m * MilliScale);
        return 0;
    }
}