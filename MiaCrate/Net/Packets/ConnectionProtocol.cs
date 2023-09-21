using MiaCrate.Net.Packets.Handshake;
using MiaCrate.Net.Packets.Status;
using Mochi.IO;

namespace MiaCrate.Net.Packets;

public class ConnectionProtocol
{
    private static readonly Dictionary<PacketState, ConnectionProtocol> _map = new();

    public static readonly ConnectionProtocol Handshake = new(PacketState.Handshake, Protocol()
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundHandshakePacket>()
        )
    );
    
    public static readonly ConnectionProtocol Status = new(PacketState.Status, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket<ServerboundStatusResponsePacket>()
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundStatusRequestPacket>()
        )
    );

    public static readonly ConnectionProtocol Login = new(PacketState.Login, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet())
        .AddFlow(PacketFlow.Serverbound, new PacketSet())
    );
    
    public static readonly ConnectionProtocol Play = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet())
        .AddFlow(PacketFlow.Serverbound, new PacketSet())
    );

    public static readonly ConnectionProtocol Configuration = new(PacketState.Configuration, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet())
        .AddFlow(PacketFlow.Serverbound, new PacketSet())
    );
    
    public static ConnectionProtocol OfState(PacketState state)
    {
        if (!_map.ContainsKey(state))
        {
            throw new NotSupportedException($"{state} not registered");
        }
        return _map[state];
    }

    public class PacketBuilder
    {
        internal Dictionary<PacketFlow, PacketSet> Flows { get; } = new();

        public PacketBuilder AddFlow(PacketFlow flow, PacketSet packets)
        {
            Flows.Add(flow, packets);
            return this;
        }
    }
    
    public PacketState PacketState { get; }
    private Dictionary<PacketFlow, PacketSet> _flows = new();

    private static PacketBuilder Protocol() => new();
    
    private ConnectionProtocol(PacketState state, PacketBuilder builder)
    {
        if (_map.ContainsKey(state))
        {
            throw new ArgumentException($"{state} already registered");
        }
        
        PacketState = state;
        _flows = builder.Flows;
        _map.Add(state, this);
    }

    public int GetPacketId(PacketFlow flow, IPacket packet) => _flows[flow].GetPacketId(packet);

    public Type GetPacketTypeById(PacketFlow flow, int id) => _flows[flow].GetPacketTypeById(id);

    private PacketSet GetPacketSetFromFlow(PacketFlow flow)
    {
        if (!_flows.ContainsKey(flow))
        {
            throw new KeyNotFoundException($"{flow} is not possible in state {PacketState}");
        }

        return _flows[flow];
    }
    
    public IPacket CreatePacket(PacketFlow flow, int id, BufferReader stream)
        => GetPacketSetFromFlow(flow).CreatePacket(id, stream);

    public T CreatePacket<T>(PacketFlow flow, int id, BufferReader stream) where T : IPacket
        => GetPacketSetFromFlow(flow).CreatePacket<T>(id, stream);
}