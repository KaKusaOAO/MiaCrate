using Mochi.Nbt;

namespace MiaCrate.Extensions;

public static class NbtExtension
{
    public static byte GetByte(this NbtCompound tag, string key) => tag[key].As<NbtByte>().Value;
}