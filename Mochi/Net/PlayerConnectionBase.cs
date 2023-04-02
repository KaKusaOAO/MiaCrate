using System.Diagnostics;
using System.Net.WebSockets;
using KaLib.Texts;
using KaLib.Utils;
using Mochi.Events;
using Mochi.Extensions;
using Mochi.Net.Packets;
using Mochi.Platforms;

namespace Mochi.Net;

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

    private Stopwatch _stopwatch = new();
    private PacketState _currentState;

    private Dictionary<Type, Action<IPacket>> _typedPacketHandlers = new();

    public void AddTypedPacketHandler<T>(Action<T> handler) where T : IPacket
    {
        _typedPacketHandlers.Add(typeof(T), p => handler((T)p));
    }

    public void ClearTypedPacketHandlers()
    {
        _typedPacketHandlers.Clear();
    }

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

    private void RawPacketIOOnOnPacketReceived(List<MemoryStream> packets)
    {
        foreach (var stream in packets)
        {
            var id = stream.ReadVarInt();
            var protocol = ConnectionProtocol.OfState(CurrentState);
            var packet = protocol.CreatePacket(ReceivingFlow, id, stream);
                    
            Logger.Verbose(TranslateText.Of("Received packet: %s")
                .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
                    
            var handler = Handlers[CurrentState];
            packet.Handle(handler);

            var type = packet.GetType();
            if (_typedPacketHandlers.ContainsKey(type))
            {
                _typedPacketHandlers[type].Invoke(packet);
            }
                    
            ReceivedPacket?.Invoke(new ReceivedPacketEventArgs
            {
                Packet = packet
            });
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
            .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
    }
}