namespace MiaCrate.Client.UI;

public interface ILayoutSettings
{
    public static ILayoutSettings Defaults => new Impl();
    
    public ILayoutSettings SetPadding(int i);
    public ILayoutSettings SetPadding(int i, int j);
    public ILayoutSettings SetPadding(int i, int j, int k, int l);

    public ILayoutSettings SetPaddingLeft(int i);
    public ILayoutSettings SetPaddingTop(int i);
    public ILayoutSettings SetPaddingRight(int i);
    public ILayoutSettings SetPaddingBottom(int i);
    
    public ILayoutSettings SetPaddingHorizontal(int i);
    public ILayoutSettings SetPaddingVertical(int i);

    public ILayoutSettings Align(float f, float g);
    public ILayoutSettings AlignHorizontally(float f);
    public ILayoutSettings AlignVertically(float f);

    public ILayoutSettings AlignHorizontallyLeft() => AlignHorizontally(0);
    public ILayoutSettings AlignHorizontallyCenter() => AlignHorizontally(0.5f);
    public ILayoutSettings AlignHorizontallyRight() => AlignHorizontally(1);

    public ILayoutSettings AlignVerticallyTop() => AlignVertically(0);
    public ILayoutSettings AlignVerticallyMiddle() => AlignVertically(0.5f);
    public ILayoutSettings AlignVerticallyBottom() => AlignVertically(1);

    public ILayoutSettings Copy();
    public Impl GetExposed();

    public class Impl : ILayoutSettings
    {
        public int PaddingLeft { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingBottom { get; set; }
        public float XAlignment { get; set; }
        public float YAlignment { get; set; }
        
        public Impl() {}

        public Impl(Impl other)
        {
            PaddingLeft = other.PaddingLeft;
            PaddingTop = other.PaddingTop;
            PaddingRight = other.PaddingRight;
            PaddingBottom = other.PaddingBottom;
            XAlignment = other.XAlignment;
            YAlignment = other.YAlignment;
        }

        public ILayoutSettings SetPadding(int i) => SetPadding(i, i);

        public ILayoutSettings SetPadding(int i, int j) => SetPaddingHorizontal(i).SetPaddingVertical(j);

        public ILayoutSettings SetPadding(int i, int j, int k, int l) =>
            SetPaddingLeft(i).SetPaddingRight(k).SetPaddingTop(j).SetPaddingBottom(l);

        public ILayoutSettings SetPaddingLeft(int i)
        {
            PaddingLeft = i;
            return this;
        }

        public ILayoutSettings SetPaddingTop(int i)
        {
            PaddingTop = i;
            return this;
        }

        public ILayoutSettings SetPaddingRight(int i)
        {
            PaddingRight = i;
            return this;
        }

        public ILayoutSettings SetPaddingBottom(int i)
        {
            PaddingBottom = i;
            return this;
        }

        public ILayoutSettings SetPaddingHorizontal(int i) => SetPaddingLeft(i).SetPaddingRight(i);

        public ILayoutSettings SetPaddingVertical(int i) => SetPaddingTop(i).SetPaddingBottom(i);

        public ILayoutSettings Align(float f, float g)
        {
            XAlignment = f;
            YAlignment = g;
            return this;
        }

        public ILayoutSettings AlignHorizontally(float f)
        {
            XAlignment = f;
            return this;
        }

        public ILayoutSettings AlignVertically(float f)
        {
            YAlignment = f;
            return this;
        }

        public ILayoutSettings Copy() => new Impl(this);

        public Impl GetExposed() => this;
    }
}