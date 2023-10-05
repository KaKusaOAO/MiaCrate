namespace MiaCrate.World;

public interface ILevelTimeAccess : ILevelReader
{
    public long DayTime { get; }

    public float GetTimeOfDay(float f) => DimensionType.TimeOfDay(DayTime);
}

public static class LevelTimeAccessExtension
{
    public static float GetTimeOfDay(this ILevelTimeAccess self, float f) => self.GetTimeOfDay(f);
}