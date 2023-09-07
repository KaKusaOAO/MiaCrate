using System.Security.Cryptography;
using System.Text;

namespace MiaCrate;

public static class UuidHelper
{
    public static Uuid CreateOfflinePlayerUuid(string name) =>
        NameUuidFromBytes(Encoding.UTF8.GetBytes($"OfflinePlayer:{name}"));

    public static Uuid NameUuidFromBytes(byte[] name)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(name);
        bytes[6] &= 0x0f; // clear version
        bytes[6] |= 0x30; // set to version 3
        bytes[8] &= 0x3f; // clear variant
        bytes[8] |= 0x80; // set to IETF variant
        return new Uuid(bytes);
    }
}