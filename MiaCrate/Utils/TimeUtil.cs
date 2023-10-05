namespace MiaCrate;

public static class TimeUtil
{
    public static UniformInt RangeOfSeconds(int min, int max) => 
        UniformInt.Of(
            SharedConstants.TicksPerSecond * min,
            SharedConstants.TicksPerSecond * max);
}