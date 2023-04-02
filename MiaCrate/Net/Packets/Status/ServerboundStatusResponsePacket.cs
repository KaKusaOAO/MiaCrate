using System.Text.Json;
using System.Text.Json.Nodes;
using Mochi.IO;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Net.Packets.Status;

public class ServerboundStatusResponsePacket : IPacket<IClientStatusPacketHandler>
{
    public IText Component { get; set; }
    
    public ServerboundStatusResponsePacket(JsonObject component)
    {
        Component = Text.FromJson(component);
    }
    
    public ServerboundStatusResponsePacket(IText component)
    {
        Component = component;
    }

    public ServerboundStatusResponsePacket(BufferReader stream)
    {
        Component = Text.FromJson(JsonSerializer.Deserialize<JsonNode>(stream.ReadUtf8String()))!;
        Logger.Info(Component.ToString());
    }
    
    public void Write(BufferWriter writer)
    {
        writer.WriteUtf8String(JsonSerializer.Serialize(Component.ToJson()));
    }

    public void Handle(IClientStatusPacketHandler handler) => handler.HandleStatusResponse(this);
}