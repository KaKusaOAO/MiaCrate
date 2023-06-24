using System.Net;
using Mochi.Utils;

namespace MiaCrate.Net;

public class SocketTranslatorServer
{
    public List<TcpSocketTranslator> Translators { get; } = new();
    public HttpListener Listener { get; } = new();
    public const ushort Port = 57142;
    
    public SocketTranslatorServer()
    {
        Listener.Prefixes.Add($"http://127.0.0.1:{Port}/translator/");
        Listener.Start();
        _ = StartEventLoopAsync();
    }

    private async Task StartEventLoopAsync()
    {
        while (Listener.IsListening)
        {
            Logger.Info("Waiting for request...");
            var context = await Listener.GetContextAsync();
            
            var request = context.Request;
            var response = context.Response;

            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);

                var destination = request.QueryString["dest"];
                if (destination == null)
                {
                    Logger.Error("Bad request! `dest` is required.");
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    response.Close();
                    continue;
                }

                var translator = new TcpSocketTranslator(webSocketContext.WebSocket, destination);
                Translators.Add(translator);
                Logger.Info("Created a packet translator");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                response.StatusCode = (int) HttpStatusCode.InternalServerError;
                response.Close();
            }
        }
    }
}