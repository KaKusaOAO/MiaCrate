using MiaCrate.Client.UI.Narration;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class CycleButton<T> : AbstractWidget
{
    private readonly IComponent _name;
    private readonly int _index;
    
    public T Value { get; private set; }
    
    private CycleButton(int x, int y, int width, int height, IComponent component, IComponent name, int index, T value) 
        : base(x, y, width, height, component)
    {
        _name = name;
        _index = index;
        Value = value;
    }

    public delegate void OnValueChange(CycleButton<T> button, T value);

    public static CycleButtonBuilder Builder(Func<T, IComponent> valueStringifier) => new(valueStringifier);
    
    public class CycleButtonBuilder
    {
        private readonly Func<T, IComponent> _valueStringifier;

        public CycleButtonBuilder(Func<T, IComponent> valueStringifier)
        {
            _valueStringifier = valueStringifier;
        }

        public CycleButton<T> Create(int x, int y, int width, int height, IComponent component)
        {
            return Create(x, y, width, height, component, (_, _) => { });
        }
        
        public CycleButton<T> Create(int x, int y, int width, int height, IComponent component, OnValueChange onValueChange)
        {
            throw new NotImplementedException();
        }
    }

    protected override void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        throw new NotImplementedException();
    }

    protected override void UpdateWidgetNarration(INarrationElementOutput output)
    {
        throw new NotImplementedException();
    }
}