namespace MiaCrate.Data.Utils;

public static class DataFixUtils
{
    public static int GetSubVersion(int key) => key % 10;
    public static int GetVersion(int key) => key / 10;
    public static int MakeKey(int version, int subVersion = 0) => version * 10 + subVersion;
}