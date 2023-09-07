namespace MiaCrate.Resources;

public enum PackPosition
{
    Top, Bottom
}

public static class PackPositionExtension
{
    public static PackPosition GetOpposite(this PackPosition pos) => pos switch
    {
        PackPosition.Bottom => PackPosition.Top,
        PackPosition.Top => PackPosition.Bottom,
        _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null)
    };

    public static int Insert<T>(this PackPosition pos, List<T> list, T obj, Func<T, Pack> func, bool opposite)
    {
        var position = opposite ? pos.GetOpposite() : pos;
        int i;
        
        if (position == PackPosition.Bottom)
        {
            for (i = 0; i < list.Count; i++)
            {
                var pack = func(list[i]);
                if (!pack.IsFixedPosition || pack.DefaultPosition != pos) break;
            }
            
            list.Insert(i, obj);
            return i;
        }


        for (i = list.Count - 1; i >= 0; i--)
        {
            var pack = func(list[i]);
            if (!pack.IsFixedPosition || pack.DefaultPosition != pos) break;
        }
        
        list.Insert(i + 1, obj);
        return i + 1;
    }
}