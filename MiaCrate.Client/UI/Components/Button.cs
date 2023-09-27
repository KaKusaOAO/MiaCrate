using MiaCrate.Client.UI.Narration;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class Button : AbstractButton
{
    protected static CreateNarrationDelegate DefaultNarration { get; } = s => s();

    private readonly OnPressDelegate _onPress;
    private readonly CreateNarrationDelegate _createNarration;
    public const int SmallWidth = 128;
    public const int DefaultWidth = 150;
    public const int DefaultHeight = 20;

    protected Button(int x, int y, int width, int height, IComponent component, 
        OnPressDelegate onPress, CreateNarrationDelegate createNarration)
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

    public static Builder CreateBuilder(IComponent message, OnPressDelegate onPress) => new(message, onPress);

    public class Builder
    {
        private readonly IComponent _message;
        private readonly OnPressDelegate _onPress;
        private CreateNarrationDelegate _createNarration;
        private int _x;
        private int _y;
        private int _width = DefaultWidth;
        private int _height = DefaultHeight;
        private Tooltip? _tooltip;

        public Builder(IComponent message, OnPressDelegate onPress)
        {
            _message = message;
            _onPress = onPress;
            _createNarration = DefaultNarration;
        }

        public Builder Pos(int x, int y)
        {
            _x = x;
            _y = y;
            return this;
        }

        public Builder Size(int width, int height)
        {
            _width = width;
            _height = height;
            return this;
        }

        public Builder Bounds(int x, int y, int width, int height) => 
            Pos(x, y).Size(width, height);

        public Builder Tooltip(Tooltip? tooltip)
        {
            _tooltip = tooltip;
            return this;
        }
        
        public Builder CreateNarration(CreateNarrationDelegate narration)
        {
            _createNarration = narration;
            return this;
        }

        public Button Build()
        {
            return new Button(_x, _y, _width, _height, _message, _onPress, _createNarration)
            {
                Tooltip = _tooltip
            };
        }
    }

    public delegate void OnPressDelegate(Button button);
    public delegate IMutableComponent CreateNarrationDelegate(Func<IMutableComponent> supplier);
}