using System.Diagnostics;
using System.Net.WebSockets;
using MiaCrate.Events;
using MiaCrate.Extensions;
using MiaCrate.Net.Packets;
using MiaCrate.Platforms;
using Mochi.IO;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Net;

public delegate Task ReceivedPacketEventAsyncDelegate(ReceivedPacketEventArgs e);
public delegate void DisconnectedEventDelegate();

public class PlayerConnectionBase
{
    public IWebSocket WebSocket { get; }
    public bool IsConnected { get; set; }
    public RawPacketIO RawPacketIO { get; }
    public PacketFlow ReceivingFlow { get; }
    protected Dictionary<PacketState, IPacketHandler> Handlers { get; } = new();

    public PacketState CurrentState
    {
        get => _currentState;
        set
        {
            Logger.Verbose($"Current state updated to {value}");
            _currentState = value;
        }
    }

    public event ReceivedPacketEventAsyncDelegate? ReceivedPacket;
    public event DisconnectedEventDelegate? Disconnected;

    private readonly Stopwatch _stopwatch = new();
    private PacketState _currentState;

    private readonly Dictionary<Type, List<Action<IPacket>>> _typedPacketHandlers = new();
    
    public PlayerConnectionBase(IWebSocket webSocket, PacketFlow receivingFlow)
    {
        WebSocket = webSocket;
        webSocket.OnClose += e =>
        {
            if (e == WebSocketCloseStatus.NormalClosure) return;
            Disconnected?.Invoke();
        };

        RawPacketIO = new RawPacketIO(webSocket);
        ReceivingFlow = receivingFlow;
        RawPacketIO.OnPacketReceived += RawPacketIOOnOnPacketReceived;
        _stopwatch.Start();
        
        if (receivingFlow != PacketFlow.Serverbound) return;
        IsConnected = true;
    }

    public void AddTypedPacketHandler<T>(Action<T> handler) where T : IPacket
    {
        var type = typeof(T);
        var list = _typedPacketHandlers.ComputeIfAbsent(type, _ => new List<Action<IPacket>>());
        list.Add(p => handler((T)p));
    }

    public void ClearTypedPacketHandlers() => 
        _typedPacketHandlers.Clear();

    public void ClearTypedPacketHandlers<T>() where T : IPacket => 
        _typedPacketHandlers.Remove(typeof(T));

    private void RawPacketIOOnOnPacketReceived(List<MemoryStream> packets)
    {
        foreach (var stream in packets.Select(s => new BufferReader(s)))
        {
            try
            {
                var id = stream.ReadVarInt();
                var protocol = ConnectionProtocol.OfState(CurrentState);
                var packet = protocol.CreatePacket(ReceivingFlow, id, stream);

                Logger.Verbose(TranslateText.Of("Received packet: %s")
                    .AddWith(Component.RepresentType(packet.GetType(), TextColor.Gold)));

                if (Handlers.TryGetValue(CurrentState, out var handler))
                {
                    packet.Handle(handler);
                    
                    var next = packet.NextProtocol;
                    if (next != null) CurrentState = next.PacketState;
                }

                var type = packet.GetType();
                foreach (var handlerType in _typedPacketHandlers.Keys.Where(handlerType => handlerType!.IsAssignableFrom(type)))
                {
                    foreach (var func in _typedPacketHandlers[handlerType])
                    {
                        func(packet);
                    }
                }

                ReceivedPacket?.Invoke(new ReceivedPacketEventArgs
                {
                    Packet = packet
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occurred while reading packet");
                Logger.Error(ex);
            } 
        }
    }
    
    public void SendPacket(IPacket packet)
    {
        if (WebSocket.State != WebSocketState.Open)
        {
            IsConnected = false;
            return;
        }
        
        var protocol = ConnectionProtocol.OfState(CurrentState);
        RawPacketIO.SendPacket(protocol, ReceivingFlow.Opposite(), packet);
        
        Logger.Verbose(TranslateText.Of("Sent packet: %s")
            .AddWith(Component.RepresentType(packet.GetType(), TextColor.Gold)));
    }
}