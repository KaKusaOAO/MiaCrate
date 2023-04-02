using System.Text.Json;
using System.Text.Json.Nodes;
using KaLib.Utils;
using Mochi.Extensions;

namespace Mochi.Net.Packets.Status;

public class ServerboundStatusResponsePacket : IPacket<IClientStatusPacketHandler>
{
    public JsonObject Component { get; set; }
    
    public ServerboundStatusResponsePacket(JsonObject component)
    {
        Component = component;
    }

    public ServerboundStatusResponsePacket(MemoryStream stream)
    {
        Component = JsonSerializer.Deserialize<JsonObject>(stream.ReadUtf8String())!;
        Logger.Info(Component.ToString());
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteUtf8String(JsonSerializer.Serialize(Component));
    }

    public void Handle(IClientStatusPacketHandler handler) => handler.HandleStatusResponse(this);
}