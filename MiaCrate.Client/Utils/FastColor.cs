namespace MiaCrate.Client.Utils;

public static class FastColor
{
    // ReSharper disable InconsistentNaming
    public static class ARGB32
    {
        public static int Alpha(int color) => color >>> 24;
        public static int Red(int color) => (color >> 16) & 0xff;
        public static int Green(int color) => (color >> 8) & 0xff;
        public static int Blue(int color) => color & 0xff;

        public static int Color(int a, int r, int g, int b) => 
            (a & 0xff) << 24 | (r & 0xff) << 16 | (g & 0xff) << 8 | (b & 0xff);
    }
}