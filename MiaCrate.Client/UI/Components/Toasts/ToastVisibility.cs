using MiaCrate.Client.Sounds;
using MiaCrate.Sounds;

namespace MiaCrate.Client.UI.Toasts;

public sealed class ToastVisibility
{
    public static ToastVisibility Show { get; } = new(SoundEvents.UiToastIn);
    public static ToastVisibility Hide { get; } = new(SoundEvents.UiToastOut);

    private readonly SoundEvent _soundEvent;

    private ToastVisibility(SoundEvent soundEvent)
    {
        _soundEvent = soundEvent;
    }

    public void PlaySound(SoundManager manager)
    {
        Util.LogFoobar();
    }
}