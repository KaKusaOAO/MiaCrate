using System.Net.WebSockets;

namespace Mochi.Platforms;

public delegate void WebSocketOpenEventDelegate();
public delegate void WebSocketMessageEventDelegate(Stream stream);
public delegate void WebSocketErrorEventDelegate();
public delegate void WebSocketCloseEventDelegate(WebSocketCloseStatus status);

public interface IWebSocket
{
    public event WebSocketOpenEventDelegate OnOpen;
    public event WebSocketMessageEventDelegate OnMessage;
    public event WebSocketErrorEventDelegate OnError;
    public event WebSocketCloseEventDelegate OnClose;

    public void Connect();
    public void Close(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? reason = null);
    public void Send(byte[] data);
    public WebSocketState State { get; }
}