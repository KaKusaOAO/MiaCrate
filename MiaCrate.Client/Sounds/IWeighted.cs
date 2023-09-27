namespace MiaCrate.Client.Sounds;

public interface IWeighted
{
    public int Weight { get; }

    public void PreloadIfRequired(SoundEngine engine);
}

public interface IWeighted<out T> : IWeighted
{
    public T GetSound(IRandomSource random);
}