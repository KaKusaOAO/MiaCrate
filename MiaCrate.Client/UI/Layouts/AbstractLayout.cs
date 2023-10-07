using Mochi.Utils;

namespace MiaCrate.Client.UI;

public abstract class AbstractLayout : ILayout
{
    private int _x;
    private int _y;

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public int X
    {
        get => _x;
        set
        {
            VisitChildren(e => { e.X += value - X; });

            _x = value;
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            VisitChildren(e => { e.Y += value - Y; });

            _y = value;
        }
    }

    protected AbstractLayout(int x, int y, int width, int height)
    {
        _x = x;
        _y = y;
        Width = width;
        Height = height;
    }

    public abstract void VisitChildren(Action<ILayoutElement> consumer);

    public void VisitWidgets(Action<AbstractWidget> consumer) => 
        ILayout.LayoutDefaults.VisitWidgets(this, consumer);

    public virtual void ArrangeElements() => 
        ILayout.LayoutDefaults.ArrangeElements(this);

    public abstract class AbstractChildWrapper
    {
        private readonly ILayoutElement _child;
        private readonly ILayoutSettings.Impl _settings;

        public int Width => _child.Width + _settings.PaddingLeft + _settings.PaddingRight;
        public int Height => _child.Height + _settings.PaddingTop + _settings.PaddingBottom;

        protected AbstractChildWrapper(ILayoutElement child, ILayoutSettings settings)
        {
            _child = child;
            _settings = settings.GetExposed();
        }

        public void SetX(int i, int j)
        {
            var f = _settings.PaddingLeft;
            var g = j - _child.Width - _settings.PaddingRight;
            var k = (int) Mth.Lerp(f, g, _settings.XAlignment);
            _child.X = k + i;
        }

        public void SetY(int i, int j)
        {
            var f = _settings.PaddingTop;
            var g = j - _child.Height - _settings.PaddingBottom;
            var k = (int) Math.Round(Mth.Lerp(f, g, _settings.YAlignment));
            _child.Y = k + i;
        }
    }
}