using System.Net.WebSockets;
using MiaCrate.Platforms;
using MiaCrate.Extensions;
using Mochi.IO;

namespace MiaCrate.Net.Packets;

public class RawPacketIO
{
    public IWebSocket WebSocket { get; }
    private MemoryStream _buffer = new();
    private bool _closed;
    private WebSocketCloseStatus _closeCode;

    public bool IsClosed => _closed;
    public WebSocketCloseStatus? CloseCode => _closed ? null : _closeCode;
    
    public event Action<List<MemoryStream>> OnPacketReceived;

    public RawPacketIO(IWebSocket webSocket)
    {
        WebSocket = webSocket;
        WebSocket.OnMessage += WebSocketOnOnMessage;
        WebSocket.OnClose += e =>
        {
            _closed = true;
            _closeCode = e;
        };
    }

    private void WebSocketOnOnMessage(Stream data)
    {
        data.CopyTo(_buffer);

        var list = new List<MemoryStream>();
        while (CanReadPacket())
        {
            list.Add(ReadRawPacket());
        }
        
        OnPacketReceived?.Invoke(list);
    }

    public void SendRawPacket(MemoryStream stream)
    {
        var len = (int) stream.Position;
        var buffer = stream.GetBuffer();
        var buf = new MemoryStream();
        var writer = new BufferWriter(buf);
        writer.WriteVarInt(len);
        buf.Write(buffer, 0, len);

        if (_closed) return;
        WebSocket.Send(buf.GetBuffer());
    }

    public void SendPacket(ConnectionProtocol protocol, PacketFlow flow, IPacket packet)
    {
        var buffer = new MemoryStream();
        var id = protocol.GetPacketId(flow, packet);
        var writer = new BufferWriter(buffer);
        writer.WriteVarInt(id);
        packet.Write(writer);

        SendRawPacket(buffer);
    }

    private bool CanReadPacket()
    {
        var total = _buffer.Position;
        if (total == 0) return false;
        
        var reader = new BufferReader(_buffer);
        _buffer.Position = 0;
        
        var len = reader.ReadVarInt();
        if (_buffer.Position >= total) return false;
        
        var result = total >= len;
        _buffer.Position = total;
        return result;
    }

    public MemoryStream ReadRawPacket()
    {
        var reader = new BufferReader(_buffer);
        
        // Reset the cursor to 0
        _buffer.Position = 0;
        
        // Read the packet content
        var buffer = reader.ReadByteArray();

        // Write remaining to a new stream
        var newBuffer = new MemoryStream();
        _buffer.CopyTo(newBuffer);
        
        // Set our buffer to the new buffer
        newBuffer.Position = 0;
        _buffer = newBuffer;
        
        return new MemoryStream(buffer);
    }
}