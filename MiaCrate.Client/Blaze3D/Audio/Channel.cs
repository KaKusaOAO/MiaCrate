using MiaCrate.Client.Sounds;

namespace MiaCrate.Client.Audio;

public class Channel : IDisposable
{
    private const int QueuedBufferCount = 4;
    public const int BufferDurationSeconds = 1;

    private const uint True = 1;
    private const uint False = 0;
    private uint _initialized = True;
    private readonly int _source;
    private IAudioStream? _stream;

    private Channel(int source)
    {
        _source = source;
    }
    
    public static Channel? Create()
    {
        Util.LogFoobar();
        return null;
    }

    public void Dispose()
    {
        Util.LogFoobar();
    }

    public void Play()
    {
        Util.LogFoobar();
    }

    private int RemoveProcessedBuffers()
    {
        Util.LogFoobar();
        return 0;
    }
}