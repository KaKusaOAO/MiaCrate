using System.Net.Sockets;
using System.Net.WebSockets;
using Mochi.IO;
using Mochi.Utils;

namespace MiaCrate.Net;

public class TcpSocketTranslator
{
    public WebSocket Source { get;  }
    public Socket DestinationSocket { get; }
    private MemoryStream _buffer = new();

    public TcpSocketTranslator(WebSocket source, string destination)
    {
        Source = source;
        DestinationSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        
        var uri = new Uri($"tcp://{destination}/");
        DestinationSocket.Connect(uri.Host, uri.Port);

        _ = StartEventLoopAsync();
    }

    private byte[] Process(byte[] buf)
    {
        var stream = new MemoryStream(buf);
        var reader = new BufferReader(stream);
        reader.ReadByteArray();

        var result = new byte[stream.Position];
        Array.Copy(buf, 0, result, 0, result.Length);
        return result;
    }

    private async Task StartEventLoopAsync()
    {
        await Task.Yield();
        
        _ = Task.Run(async () =>
        {
            while (DestinationSocket.Connected && !Source.CloseStatus.HasValue)
            {
                try
                {
                    var buffer = new byte[40960];
                    await Source.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    buffer = Process(buffer);
                    await DestinationSocket.SendAsync(buffer, SocketFlags.None);

                    if (Source.CloseStatus.HasValue)
                    {
                        await Source.CloseAsync(Source.CloseStatus.Value, "", CancellationToken.None);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception occurred in writer task!");
                    Logger.Error(ex);
                }
            }
            
            Logger.Warn("Out of writer loop");
        });

        _ = Task.Run(async () =>
        {
            while (DestinationSocket.Connected && !Source.CloseStatus.HasValue)
            {
                try
                {
                    var buffer = new byte[40960];
                    SpinWait.SpinUntil(() => DestinationSocket.Available > 0, 500);
                    var count = await DestinationSocket.ReceiveAsync(buffer, SocketFlags.None);
                    if (count == 0) continue;
                    _buffer.Write(buffer, 0, count);

                    while (CanReadPacket())
                    {
                        var b = new MemoryStream();
                        var read = ReadRawPacket();
                        await read.CopyToAsync(b);

                        var arr = b.GetBuffer().Take((int)b.Position).ToArray();
                        b = new MemoryStream();
                        var writer = new BufferWriter(b);
                        writer.WriteByteArray(arr);

                        var buf = new byte[b.Position];
                        Array.Copy(b.GetBuffer(), 0, buf, 0, buf.Length);
                        await Source.SendAsync(buf, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception occurred in reader task!");
                    Logger.Error(ex);
                }
            }
            
            Logger.Warn("Out of reader loop");
        });

        SpinWait.SpinUntil(() => !(DestinationSocket.Connected && !Source.CloseStatus.HasValue));
        Logger.Warn("Out of event loop");
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
        
        // Read the packet length
        var len = reader.ReadVarInt();

        // Read the packet content
        var buffer = new byte[len];
        _buffer.Read(buffer, 0, len);

        // Write remaining to a new stream
        var newBuffer = new MemoryStream();
        _buffer.CopyTo(newBuffer);
        
        // Set our buffer to the new buffer
        newBuffer.Position = 0;
        _buffer = newBuffer;
        
        return new MemoryStream(buffer);
    }
}