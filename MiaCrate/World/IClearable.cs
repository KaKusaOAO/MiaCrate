namespace MiaCrate.World;

public interface IClearable
{
    void ClearContent();

    // Why would I need this?
    public static void TryClear(object obj)
    {
        if (obj is IClearable clearable)
        {
            clearable.ClearContent();
        }
    }
}