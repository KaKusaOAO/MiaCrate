// See https://aka.ms/new-console-template for more information

using KaLib.Utils;
using Mochi;
using Mochi.Net;
using Mochi.Net.Packets;
using Mochi.Net.Packets.Handshake;
using Mochi.Net.Packets.Status;
using Mochi.Platforms;

Logger.Logged += Logger.LogToEmulatedTerminalAsync;
MochiCore.Bootstrap(IPlatform.Default);

var server = new SocketTranslatorServer();
await Task.Delay(1000);

var host = "mc.hypixel.net";
ushort port = 25565;

var client = MochiCore.Platform.CreateClient(new Uri($"ws://127.0.0.1:57142/translator/?dest={host}:{port}"));
client.Connect();

var conn = new PlayerConnectionBase(client, PacketFlow.Clientbound);
conn.SendPacket(new ServerboundHandshakePacket(762, host, port, PacketState.Status));
conn.CurrentState = PacketState.Status;
conn.SendPacket(new ServerboundStatusRequestPacket());
await Task.Delay(-1);
