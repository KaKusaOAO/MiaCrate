namespace MiaCrate.Client.UI.Toasts;

public class ToastComponent
{
    private const int SlotCount = 5;
    private const int NoSpace = -1;

    private readonly Game _game;
    
    public ToastComponent(Game game)
    {
        _game = game;
    }

    public void Render(GuiGraphics graphics)
    {
        throw new NotImplementedException();
    }

    public class ToastInstance<T> where T : IToast
    {
        private readonly ToastComponent _instance;
        private readonly int _index;
        private readonly int _slotCount;

        private long _animationTime = -1;
        private long _visibleTime = -1;
        private ToastVisibility _visibility = ToastVisibility.Show;

        public T Toast { get; }
        
        public ToastInstance(ToastComponent instance, T toast, int index, int slotCount)
        {
            _instance = instance;
            Toast = toast;
            _index = index;
            _slotCount = slotCount;
        }

        public bool Render(int i, GuiGraphics graphics)
        {
            throw new NotImplementedException();
        }
    }
}