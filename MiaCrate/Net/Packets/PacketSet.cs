using System.Diagnostics.CodeAnalysis;
using Mochi.IO;

namespace MiaCrate.Net.Packets;

public class PacketSet
{
    private readonly Dictionary<int, Type> _packetMap = new();
    private readonly Dictionary<Type, int> _packetIdMap = new();
    private readonly Dictionary<int, Func<BufferReader, IPacket>> _deserializers = new();

    public PacketSet AddPacket
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]T>() 
        where T : IPacket => AddPacketAndDefaultDeserializer(typeof(T));

    public PacketSet AddPacket<T>(Func<BufferReader, T> deserializer) where T : IPacket
    {
        AddPacket(typeof(T));
        _deserializers.Add(_packetIdMap[typeof(T)], s => deserializer(s));
        return this;
    }

    public PacketSet AddPacket(Type type)
    {
        if (!typeof(IPacket).IsAssignableFrom(type))
        {
            throw new ArgumentException($"{type} is not a Packet type", nameof(type));
        }

        var id = _packetMap.Count;
        _packetMap.Add(id, type);
        _packetIdMap.Add(type, id);
        return this;
    }
    
    public PacketSet AddPacketAndDefaultDeserializer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
    {
        var set = AddPacket(type);
        var ctor = type.GetConstructor(new[] { typeof(BufferReader) });
        if (ctor == null)
        {
            throw new NotSupportedException($"The MemoryStream constructor in type {type} is not defined");
        }

        var id = _packetIdMap[type];
        _deserializers.Add(id, stream => (IPacket) ctor.Invoke(new object[] { stream }));
        return set;
    }

    public Type GetPacketTypeById(int id)
    {
        if (!_packetMap.ContainsKey(id))
        {
            throw new ArgumentException($"Bad packet ID {id}");
        }
        
        return _packetMap[id];
    }
    
    public Func<BufferReader, IPacket> GetDeserializerById(int id)
    {
        if (_deserializers.TryGetValue(id, out var byId))
            return byId;

        throw new InvalidOperationException("The deserializer of the type is not registered");
    }

    public int GetPacketId(IPacket packet)
    {
        var type = packet.GetType();
        if (!_packetIdMap.ContainsKey(type))
        {
            throw new ArgumentException($"Packet type {type} not registered");
        }

        return _packetIdMap[type];
    }

    public IPacket CreatePacket(int id, BufferReader stream) => GetDeserializerById(id)(stream);

    public T CreatePacket<T>(int id, BufferReader stream) where T : IPacket =>
        (T) CreatePacket(id, stream);
}