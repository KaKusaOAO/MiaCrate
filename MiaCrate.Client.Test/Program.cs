// See https://aka.ms/new-console-template for more information

using MiaCrate;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Nbt;
using MiaCrate.Net;
using MiaCrate.Net.Packets;
using MiaCrate.Net.Packets.Handshake;
using MiaCrate.Net.Packets.Status;
using Mochi.Utils;
using IPlatform = MiaCrate.Platforms.IPlatform;

Logger.Logged += Logger.LogToEmulatedTerminalAsync; // LogToEmulatedTerminalAsync;
Logger.RunThreaded();

MiaCore.Bootstrap(IPlatform.Default);
_ = new SocketTranslatorServer();

var host = "mc.hypixel.net";
ushort port = 25565;

var client = MiaCore.Platform.CreateClient(
    new Uri($"ws://127.0.0.1:{SocketTranslatorServer.Port}/translator/?dest={host}:{port}"));
client.Connect();

var ops = new NbtOps();
var codec = RecordCodecBuilder.Create<TestRecord>(data =>
    data.Group(
            Codec.Byte.FieldOf("byte").ForGetter<TestRecord>(r => r.Byte),
            Codec.Bool.FieldOf("flag").ForGetter<TestRecord>(r => r.Flag))
        .Apply(data, (b, f) => new TestRecord(b, f))
);

var record = new TestRecord((byte)Random.Shared.Next(byte.MaxValue), true);
Logger.Info($"Encoding test record: {record}");

var result = codec.Encode(record, ops, ops.Empty);
result.Result.IfPresent(tag =>
{
    Logger.Info("Encoded test record:");
    Logger.Info(tag.ToString());
}).IfEmpty(() =>
{
    Logger.Error("Encoding failed!");
    result.Get().IfRight(r => Logger.Error(r.Message));
});

var conn = new PlayerConnectionBase(client, PacketFlow.Clientbound);
conn.AddTypedPacketHandler<ServerboundStatusResponsePacket>(p =>
{
    Logger.Info("Received status response:");
    Logger.Info(p.Component);
});

conn.SendPacket(new ServerboundHandshakePacket(763, host, port, PacketState.Status));
conn.CurrentState = PacketState.Status;
conn.SendPacket(new ServerboundStatusRequestPacket());
await Task.Delay(-1);

record TestRecord(byte Byte, bool Flag);