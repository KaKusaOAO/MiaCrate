namespace MiaCrate.Client.UI;

public interface ILayout : ILayoutElement
{
    public void VisitChildren(Action<ILayoutElement> consumer);

    public new void VisitWidgets(Action<AbstractWidget> consumer) => 
        LayoutDefaults.VisitWidgets(this, consumer);

    void ILayoutElement.VisitWidgets(Action<AbstractWidget> consumer) => VisitWidgets(consumer);

    public void ArrangeElements() => 
        LayoutDefaults.ArrangeElements(this);

    protected static class LayoutDefaults
    {
        public static void VisitWidgets(ILayout layout, Action<AbstractWidget> consumer)
        {
            layout.VisitChildren(e =>
            {
                e.VisitWidgets(consumer);
            });
        }
        
        public static void ArrangeElements(ILayout layout)
        {
            layout.VisitChildren(e =>
            {
                if (e is ILayout layout) 
                    layout.ArrangeElements();
            });
        }
    }
}