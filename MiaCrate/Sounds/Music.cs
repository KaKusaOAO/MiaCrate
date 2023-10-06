using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Sounds;

public class Music
{
    public static ICodec<Music> Codec { get; } =
        RecordCodecBuilder.Create<Music>(instance => instance
            .Group(
                SoundEvent.Codec.FieldOf("sound").ForGetter<Music>(m => m.Event),
                Data.Codec.Int.FieldOf("min_delay").ForGetter<Music>(m => m.MinDelay),
                Data.Codec.Int.FieldOf("max_delay").ForGetter<Music>(m => m.MaxDelay),
                Data.Codec.Bool.FieldOf("replace_current_music").ForGetter<Music>(m => m.CanReplaceCurrentMusic)
            )
            .Apply<Music>(instance)
        );

    public IHolder<SoundEvent> Event { get; }
    public int MinDelay { get; }
    public int MaxDelay { get; }
    public bool CanReplaceCurrentMusic { get; }

    public Music(IHolder<SoundEvent> soundEvent, int minDelay, int maxDelay, bool replaceCurrentMusic)
    {
        Event = soundEvent;
        MinDelay = minDelay;
        MaxDelay = maxDelay;
        CanReplaceCurrentMusic = replaceCurrentMusic;
    }
}