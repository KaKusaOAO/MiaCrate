using MiaCrate.Core;
using MiaCrate.Sounds;

namespace MiaCrate.Client.Resources;

public class SimpleSoundInstance : AbstractSoundInstance
{
    public SimpleSoundInstance(ResourceLocation location, SoundSource source, float volume, float pitch, IRandomSource random,
        bool looping, int delay, AttenuationType attenuation, double x, double y, double z, bool relative) 
        : base(location, source, random)
    {
        Volume = volume;
        Pitch = pitch;
        X = x;
        Y = y;
        Z = z;
        IsLooping = looping;
        Delay = delay;
        Attenuation = attenuation;
        IsRelative = relative;
    }

    public static SimpleSoundInstance ForUi(SoundEvent soundEvent, float pitch) => 
        ForUi(soundEvent, pitch, 0.25f);

    public static SimpleSoundInstance ForUi(IHolder<SoundEvent> holder, float pitch) =>
        ForUi(holder.Value, pitch);

    public static SimpleSoundInstance ForUi(SoundEvent soundEvent, float pitch, float volume) =>
        new(soundEvent.Location, SoundSource.Master, volume, pitch, ISoundInstance.CreateUnseededRandom(), false, 0,
            AttenuationType.None, 0, 0, 0, true);
}