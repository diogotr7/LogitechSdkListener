using EmbedIO.WebSockets;
using Logi.Protocol;

namespace LogitechSdkListener;

internal class LogitechWebSocketModule : WebSocketModule
{
    public LogitechWebSocketModule() : base("/",true)
    {
        AddProtocol("protobuf");
    }

    protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        Task.Run(() =>
        {
            var message = Message.Parser.ParseFrom(buffer);
            var payload = message.Payload.Unpack(LogitechSdkTypeRegistry.TypeRegistry);
            Console.WriteLine(payload);
            
            //TODO: figure out how to send a response
        });
            
        return Task.CompletedTask;
    }
}