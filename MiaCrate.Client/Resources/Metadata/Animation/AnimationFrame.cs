namespace MiaCrate.Client.Resources;

public class AnimationFrame
{
    public const int UnknownFrameTime = -1;
    
    public int Index { get; }
    public int Time { get; }

    public AnimationFrame(int index, int time = UnknownFrameTime)
    {
        Index = index;
        Time = time;
    }
}