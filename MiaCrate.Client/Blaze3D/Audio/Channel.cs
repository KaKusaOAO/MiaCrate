using MiaCrate.Client.Sounds;
using Mochi.Utils;
using OpenTK.Audio.OpenAL;

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
        var i = 0;
        AL.GenSources(1, ref i);
        return OpenAlUtil.CheckAlError("Allocate new source")
            ? null
            : new Channel(i);
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _initialized, False, True) != True) 
            return;
        
        AL.SourceStop(_source);
        OpenAlUtil.CheckAlError("Stop");

        if (_stream != null)
        {
            try
            {
                _stream.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to close audio stream");
                Logger.Error(ex);
            }

            RemoveProcessedBuffers();
        }

        unsafe
        {
            fixed (int* source = &_source)
            {
                AL.DeleteSources(1, source);
                OpenAlUtil.CheckAlError("Cleanup");
            }
        }
    }

    public void Play()
    {
        AL.SourcePlay(_source);
    }

    private int RemoveProcessedBuffers()
    {
        var i = AL.GetSource(_source, ALGetSourcei.BuffersProcessed);
        if (i <= 0) return i;

        var arr = new int[i];
        AL.SourceUnqueueBuffers(_source, arr);
        OpenAlUtil.CheckAlError("Unqueue buffers");
        
        AL.DeleteBuffers(arr);
        OpenAlUtil.CheckAlError("Remove processed buffers");

        return i;
    }
}