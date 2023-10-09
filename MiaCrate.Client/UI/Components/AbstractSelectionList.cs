using MiaCrate.Client.UI.Narration;
using MiaCrate.Client.UI.Screens;

namespace MiaCrate.Client.UI;

public abstract class AbstractSelectionList<T> : AbstractContainerEventHandler, IRenderable, INarratableEntry 
    where T : AbstractSelectionList<T>.Entry
{
    protected const int ScrollBarWidth = 6;

    private static ResourceLocation ScrollerSprite { get; } = new("widget/scroller"); 
    
    private readonly Game _game;
    private readonly List<T> _children = new();
    private bool _renderBackground = true;
    private double _scrollAmount;
    private int _headerHeight;

    protected int Width { get; set; }
    protected int Height { get; set; }
    protected int Y0 { get; set; }
    protected int Y1 { get; set; }
    protected int ItemHeight { get; set; }
    protected int X0 { get; set; }
    protected int X1 { get; set; }
    public virtual int RowWidth => 220;
    public virtual int RowLeft => X0 + Width / 2 - RowWidth / 2 + 2;
    public virtual int RowRight => RowLeft + RowWidth;
    
    public T? SelectedChild { get; set; }
    
    public T? HoveredChild { get; set; }

    protected virtual int ItemCount => _children.Count;

    public virtual double ScrollAmount
    {
        get => _scrollAmount;
        set => _scrollAmount = Math.Clamp(value, 0, MaxScroll);
    }

    protected virtual int MaxPosition => ItemCount * ItemHeight + _headerHeight;

    public int MaxScroll => Math.Max(0, MaxPosition - (Y1 - Y0 - 4));

    public override List<IGuiEventListener> Children => _children.Cast<IGuiEventListener>().ToList();

    protected virtual int ScrollBarPosition => Width / 2 + 124;

    public NarrationPriority NarrationPriority
    {
        get
        {
            if (IsFocused)
                return NarrationPriority.Focused;

            if (HoveredChild != null)
                return NarrationPriority.Hovered;

            return NarrationPriority.None;
        }
    }

    protected AbstractSelectionList(Game game, int width, int height, int y0, int y1, int itemHeight)
    {
        _game = game;
        
        Width = width;
        Height = height;
        Y0 = y0;
        Y1 = y1;
        ItemHeight = itemHeight;
        X0 = 0;
        X1 = width;
    }

    protected void ClearEntries()
    {
        _children.Clear();
        SelectedChild = null;
    }

    protected T GetEntry(int i) => _children[i];

    protected virtual int AddEntry(T entry)
    {
        _children.Add(entry);
        return _children.Count - 1;
    }

    protected virtual bool IsSelectedItem(int i) => 
        Equals(SelectedChild, _children[i]);

    public void UpdateSize(int width, int height, int y0, int y1)
    {
        Width = width;
        Height = height;
        Y0 = y0;
        Y1 = y1;
        X0 = 0;
        X1 = width;
    }

    public void SetLeftPos(int i)
    {
        X0 = i;
        X1 = i + Width;
    }

    public void SetRenderBackground(bool bl)
    {
        _renderBackground = bl;
    }

    public void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        if (_renderBackground)
        {
            graphics.SetColor(0.125f, 0.125f, 0.125f, 1);
            graphics.Blit(Screen.BackgroundLocation, X0, Y0, X1, Y1 + (int) ScrollAmount, X1 - X0, Y1 - Y0, 32, 32);
            graphics.SetColor(1, 1, 1, 1);
        }
        
        EnableScissor(graphics);

        RenderList(graphics, mouseX, mouseY, f);
        
        graphics.DisableScissor();
    }

    protected virtual void EnableScissor(GuiGraphics graphics)
    {
        graphics.EnableScissor(X0, Y0, X1, Y1);
    }

    protected void RenderList(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var k = RowLeft;
        var l = RowWidth;
        var m = ItemHeight - 4;
        var n = ItemCount;

        for (var o = 0; o < n; o++)
        {
            var p = GetRowTop(o);
            var q = GetRowBottom(o);

            if (q >= Y0 && p <= Y1)
            {
                RenderItem(graphics, mouseX, mouseY, f, o, k, p, l, m);
            }
        }
    }

    protected virtual void RenderItem(GuiGraphics graphics, int mouseX, int mouseY, float f, int k, int l, int m, int n, int o)
    {
        var entry = GetEntry(k);
        entry.RenderBack(graphics, k, m, l, n, o, mouseX, mouseY, Equals(HoveredChild == entry), f);
        entry.Render(graphics, k, m, l, n, o, mouseX, mouseY, Equals(HoveredChild == entry), f);
    }

    protected virtual int GetRowTop(int i) => Y0 + 4 - (int) ScrollAmount + i * ItemHeight + _headerHeight;

    protected virtual int GetRowBottom(int i) => GetRowTop(i) + ItemHeight;

    public abstract void UpdateNarration(INarrationElementOutput output);

    public abstract class Entry : IGuiEventListener
    {
        public AbstractSelectionList<T> List { get; set; }

        public virtual bool IsFocused
        {
            get => List.FocusedChild == this;
            set {}
        }

        public abstract void Render(GuiGraphics graphics, int i, int j, int k, int l, int m, int n, int o, bool bl,
            float f);

        public virtual void RenderBack(GuiGraphics graphics, int i, int j, int k, int l, int m, int n, int o, bool bl,
            float f) {}

        public bool IsMouseOver(double x, double y) => false;
    }
}