namespace MiaCrate.Client.Graphics;

public interface ISpriteTicker : IDisposable
{
    public void TickAndUpload(int i, int j);
}