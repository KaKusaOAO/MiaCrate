using MiaCrate.Core;

namespace MiaCrate.Sounds;

public static class Musics
{
    private const int OneSecond = SharedConstants.TicksPerSecond;
    private const int ThirtySeconds = SharedConstants.TicksPerSecond * 30;
    private const int TenMinutes = SharedConstants.TicksPerMinute * 10;
    private const int TwentyMinutes = SharedConstants.TicksPerMinute * 20;
    private const int FiveMinutes = SharedConstants.TicksPerMinute * 5;

    public static Music Menu { get; } = new(SoundEvents.MusicMenu, OneSecond, ThirtySeconds, true);
    public static Music Creative { get; } = new(SoundEvents.MusicCreative, TenMinutes, TwentyMinutes, false);
    public static Music Credits { get; } = new(SoundEvents.MusicCredits, 0, 0, true);
    public static Music EndBoss { get; } = new(SoundEvents.MusicDragon, 0, 0, true);
    public static Music End { get; } = new(SoundEvents.MusicEnd, 0, 0, true);
    public static Music Underwater { get; } = CreateGameMusic(SoundEvents.MusicUnderwater);
    public static Music Game { get; } = CreateGameMusic(SoundEvents.MusicGame);
    
    public static Music CreateGameMusic(IHolder<SoundEvent> holder) => 
        new(holder, TenMinutes, TwentyMinutes, false);
}