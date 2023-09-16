using MiaCrate.Client.UI.Narration;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class Button : AbstractButton
{
    private readonly Action<Button> _onPress;
    private readonly CreateNarration _createNarration;
    public const int SmallWidth = 128;
    public const int DefaultWidth = 150;
    public const int DefaultHeight = 20;

    public delegate IMutableComponent CreateNarration(Func<IMutableComponent> supplier);

    protected Button(int x, int y, int width, int height, IComponent component, 
        Action<Button> onPress, CreateNarration createNarration)
        : base(x, y, width, height, component)
    {
        _onPress = onPress;
        _createNarration = createNarration;
    }

    public override void OnPress() => _onPress(this);

    protected override IMutableComponent CreateNarrationMessage() => 
        _createNarration(() => base.CreateNarrationMessage());

    protected override void UpdateWidgetNarration(INarrationElementOutput output)
    {
        throw new NotImplementedException();
    }
}