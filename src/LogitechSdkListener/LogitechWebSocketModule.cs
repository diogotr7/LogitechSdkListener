using EmbedIO.WebSockets;
using Logi.Protocol;
using Google.Protobuf;

namespace LogitechSdkListener;

internal class LogitechWebSocketModule : WebSocketModule
{
    public LogitechWebSocketModule() : base("/",false)
    {
        AddProtocol("protobuf");
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        var message = Message.Parser.ParseFrom(buffer);
        var payload = message.Payload?.Unpack(LogitechSdkTypeRegistry.TypeRegistry);
            
        Console.WriteLine($"Received message {message} with payload {payload}");

        //TODO: figure out how to send a response
        var response = new Message()
        {
            MsgId = Guid.NewGuid().ToString(),
            Verb = message.Verb,
            Path = message.Path,
            Origin = "backend",
            Payload = message.Payload,
            Result = new Result() { Code = Result.Types.Code.Success }
        };

        await context.WebSocket.SendAsync(response.ToByteArray(), false);
    }

    protected override async Task OnClientConnectedAsync(IWebSocketContext context)
    {
        //todo: is this correct?
        var initMessage = new Message()
        {
            Verb = Message.Types.Verb.Options,
            Path = "/",
            Origin = "backend",
        };
        await context.WebSocket.SendAsync(initMessage.ToByteArray(), false);
        
        await base.OnClientConnectedAsync(context);
    }
    
}