using MiaCrate.Extensions;

namespace MiaCrate.Net.Data;

public static class EntityDataSerializers
{
    private static readonly List<IEntityDataSerializer> _serializers = new();

    public static readonly IEntityDataSerializer<byte> Byte = EntityDataSerializer.MakeSimple(
        (s, b) => s.WriteByte(b),
        s =>
        {
            var r = s.ReadByte();
            if (r == -1)
                throw new EndOfStreamException();
            return (byte)r;
        });
    
    public static readonly IEntityDataSerializer<int> Int = EntityDataSerializer.MakeSimple(
        (s, i) => s.WriteVarInt(i),
        s => s.ReadVarInt());
    
    public static readonly IEntityDataSerializer<bool> Bool = EntityDataSerializer.MakeSimple(
        (s, b) => s.WriteBool(b),
        s => s.ReadBool());

    private static void Register(IEntityDataSerializer serializer)
    {
        _serializers.Add(serializer);
    }

    static EntityDataSerializers()
    {
        Register(Byte);
        Register(Int);
        Register(Bool);
    }
    
    public static IEntityDataSerializer Get(int id) => _serializers[id];
    
    public static IEntityDataSerializer<T> Get<T>(int id) => (IEntityDataSerializer<T>) _serializers[id];
    
    public static int GetId(IEntityDataSerializer serializer) => _serializers.IndexOf(serializer);
}