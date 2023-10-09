using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Resources;

public interface IPackSource
{
    public static readonly Func<IComponent, IComponent> NoDecoration = x => x;
    
    public static IPackSource Default { get; } = Create(NoDecoration, true);
    public static IPackSource BuiltIn { get; } = Create(DecorateWithSource("pack.source.builtin"), true);
    public static IPackSource Feature { get; } = Create(DecorateWithSource("pack.source.feature"), false);
    public static IPackSource World { get; } = Create(DecorateWithSource("pack.source.world"), true);
    public static IPackSource Server { get; } = Create(DecorateWithSource("pack.source.server"), true);
    
    public IComponent Decorate(IComponent component);
    public bool ShouldAddAutomatically { get; }

    public static IPackSource Create(Func<IComponent, IComponent> decorate, bool shouldAutoAdd) =>
        new Simple(decorate, shouldAutoAdd);

    private static Func<IComponent, IComponent> DecorateWithSource(string key)
    {
        var text = TranslateText.Of(key);
        return c => TranslateText.Of("pack.nameAndSource", c, text)
            .WithColor(TextColor.Gray);
    }

    private class Simple : IPackSource
    {
        private readonly Func<IComponent, IComponent> _decorate;
        private readonly bool _shouldAutoAdd;

        public Simple(Func<IComponent, IComponent> decorate, bool shouldAutoAdd)
        {
            _decorate = decorate;
            _shouldAutoAdd = shouldAutoAdd;
        }

        public IComponent Decorate(IComponent component) => _decorate(component);
        public bool ShouldAddAutomatically => _shouldAutoAdd;
    }
}