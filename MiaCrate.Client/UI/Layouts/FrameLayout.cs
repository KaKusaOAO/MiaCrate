using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class FrameLayout : AbstractLayout
{
    public FrameLayout() : this(0, 0, 0, 0) {}
    
    public FrameLayout(int width, int height) : this(0, 0, width, height) {}
    
    public FrameLayout(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }

    public override void VisitChildren(Action<ILayoutElement> consumer)
    {
        throw new NotImplementedException();
    }

    public static void AlignInRectangle(ILayoutElement element, int i, int j, int k, int l, float f, float g)
    {
        AlignInDimension(i, k, element.Width, x => element.X = x, f);
        AlignInDimension(j, l, element.Height, y => element.Y = y, g);
    }

    public static void AlignInDimension(int i, int j, int k, Action<int> consumer, float f)
    {
        var l = (int) Mth.Lerp(0, j - k, f);
        consumer(i + l);
    }
}