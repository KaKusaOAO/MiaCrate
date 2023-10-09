using MiaCrate.Client.UI;

namespace MiaCrate.Client.IrisShader.UI;

public class ShaderPackSelectionList : IrisObjectSelectionList<ShaderPackSelectionList.BaseEntry>
{
    private readonly ShaderPackScreen _screen;

    public override int RowWidth => Math.Min(308, Width - 50);

    public ShaderPackSelectionList(ShaderPackScreen screen, Game game, int width, int height, int y0, int y1, int x0, int x1) 
        : base(game, width, height, y0, y1, x0, x1, Button.DefaultHeight)
    {
        _screen = screen;
    }

    protected override int GetRowTop(int i) => base.GetRowTop(i) + 2;

    public abstract class BaseEntry : Entry
    {
        protected BaseEntry() {}
    }

    public class ShaderPackEntry : BaseEntry
    {
        // Always add additional field for non-static Java class
        private readonly ShaderPackSelectionList _instance;
        
        private readonly int _index;
        private readonly ShaderPackSelectionList _list;
        private readonly string _packName;

        public ShaderPackEntry(ShaderPackSelectionList instance, int index, 
            ShaderPackSelectionList list,
            string packName)
        {
            _instance = instance;
            _index = index;
            _list = list;
            _packName = packName;
        }
        
        public override void Render(GuiGraphics graphics, int i, int j, int k, int l, int m, int n, int o, bool bl, float f)
        {
            
        }
    }
}