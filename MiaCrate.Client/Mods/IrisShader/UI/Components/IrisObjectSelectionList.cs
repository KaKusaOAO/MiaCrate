using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Narration;

namespace MiaCrate.Client.IrisShader.UI;

public class IrisObjectSelectionList<T> : AbstractSelectionList<T> where T : AbstractSelectionList<T>.Entry
{
    protected override int ScrollBarPosition => Width - 6;

    public IrisObjectSelectionList(Game game, int width, int height, int y0, int y1, int x0, int x1, int itemHeight) 
        : base(game, width, height, y0, y1, itemHeight)
    {
        X0 = x0;
        X1 = x1;
    }

    public override void UpdateNarration(INarrationElementOutput output)
    {
        
    }
}