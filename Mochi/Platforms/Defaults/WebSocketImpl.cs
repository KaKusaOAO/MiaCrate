using System.Net.WebSockets;

namespace Mochi.Platforms.Defaults;

public class WebSocketImpl : IWebSocket
{
    private MemoryStream _buffer = new();
    
    public Uri? Destination { get; }
    public WebSocket WebSocket { get; }

    private WebSocketImpl(WebSocket webSocket, Uri? destination = null)
    {
        WebSocket = webSocket;
        Destination = destination;
    }

    public static IWebSocket CreateClient(Uri uri)
    {
        var client = new ClientWebSocket();
        return new WebSocketImpl(client, uri);
    }

    public static IWebSocket CreateFromExisting(WebSocket webSocket)
    {
        return new WebSocketImpl(webSocket);
    }

    public event WebSocketOpenEventDelegate? OnOpen;
    public event WebSocketMessageEventDelegate? OnMessage;
    public event WebSocketErrorEventDelegate? OnError;
    public event WebSocketCloseEventDelegate? OnClose;

    public void Connect()
    {
        if (Destination == null)
            throw new InvalidOperationException("The destination is not set. Is this a server-side WebSocket?");

        if (WebSocket is not ClientWebSocket client)
            throw new NotSupportedException($"Unsupported client: {WebSocket}");
        
        Task.Run(async () =>
        {
            await client.ConnectAsync(Destination, new CancellationTokenSource(TimeSpan.FromSeconds(6)).Token);
            OnOpen?.Invoke();

            _ = RunEventLoopAsync();
        }).Wait();
    }

    public void Close(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? reason = null)
    {
        Task.Run(async () =>
        {
            await WebSocket.CloseAsync(status, reason ?? "", CancellationToken.None);
        }).Wait();
    }

    private async Task RunEventLoopAsync()
    {
        while (!WebSocket.CloseStatus.HasValue)
        {
            var buffer = new byte[40960];
            var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _buffer.Write(buffer, 0, result.Count);

            if (WebSocket.CloseStatus.HasValue)
            {
                await WebSocket.CloseAsync(WebSocket.CloseStatus.Value, "", CancellationToken.None);
            }

            if (!result.EndOfMessage) continue;
            
            var arr = _buffer.GetBuffer();
            var data = new byte[_buffer.Position];
            Array.Copy(arr, 0, data, 0, data.Length);
            OnMessage?.Invoke(new MemoryStream(data));
            _buffer = new MemoryStream();
        }
    }

    public void Send(byte[] data)
    {
        Task.Run(async () =>
        {
            if (WebSocket.CloseStatus.HasValue) return;
            await WebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
        }).Wait();
    }

    public WebSocketState State => WebSocket.State;
}