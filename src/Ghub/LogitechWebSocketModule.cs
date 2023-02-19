using System.Diagnostics;
using EmbedIO.WebSockets;
using Logi.Protocol;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Logi.Protocol.Applications;
using Logi.Protocol.Integrations;
using Swan.Logging;

namespace LogitechSdkListener;

internal class LogitechWebSocketModule : WebSocketModule
{
    private string appPath;
    private string appName;
    private string appGuid;
    
    public LogitechWebSocketModule() : base("/", false)
    {
        AddProtocol("protobuf");
        
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
        IWebSocketReceiveResult result)
    {
        var message = Message.Parser.ParseFrom(buffer);
        var payload = message.Payload?.Unpack(LogitechSdkTypeRegistry.TypeRegistry);
        Logger.Info($"Received message {message} with payload {payload}");
        if (payload is null)
            return;
        
        Logger.Info(message.Path);
        if (message.Path == "/api/v1/integration/register")
        {
            var sdkInt = payload as SDKIntegration;
            appPath = sdkInt?.AppPath;
            appName = sdkInt?.Name;
            Logger.Info("Received register message");
            await SendRegisteredResponse(context, message.Payload);
        }
        else if (message.Path == "/api/v1/integration/activate")
        {
            var instanceInfo = payload as InstanceInfo;
            appGuid = instanceInfo.IntegrationGuid;
            Logger.Info("Received activate message");
            await SendActivatedResponse(context, instanceInfo);
        }
        else if (message.Path == "/api/v1/integration")
        {
            var integration = new Integration()
            {
                TargetApplication = new Application
                {
                    ApplicationPath = appPath//saved path from register
                },
                // SupportedApplicationIds =
                // {
                //     ""//TODO: some random guid??
                // },
                Name = appName,//saved name from register
                //IconUrl = "",//TODO: some url, is this even needed?
                //Poster = "",//TODO: poster url, is this even needed?
                Guid = appGuid,//saved guid from Activate
                Enabled = true,
                LaunchType = Integration.Types.LaunchType.Manual,
                // IndividualApplicationPreferences =
                // {
                //     [""] = new Integration.Types.ApplicationPreferences()
                //     {
                //         Enabled = true//same guid from SupportedApplicationIds??
                //     }
                // },
                LedSdk = new Integration.Types.LED_SDK_Entry()
                {
                    Enabled = true
                }                    
            };
            var res = new Message()
            {
                MsgId = Guid.NewGuid().ToString(),
                Verb = Message.Types.Verb.Get,
                Path = "/api/v1/integration",
                Origin = "backend",
                Result = new Result { Code = Result.Types.Code.Success },
                Payload = Any.Pack(integration, "type.googleapis.com/logi.protocol.integrations.Integration"),
            };
            //var bytes = File.ReadAllBytes("dumps/06-integration-out.bin");
            //var message2 = Message.Parser.ParseFrom(bytes);
            //await SendAsync(context, bytes);
            await SendAsync(context, res.ToByteArray());
            //todo: send integration info
        }

        //await context.WebSocket.SendAsync(response.ToByteArray(), false);
    }

    private async Task SendActivatedResponse(IWebSocketContext context, InstanceInfo payload)
    {
        payload.InstanceGuid = Guid.NewGuid().ToString();
        var activatedResponse = new Message()
        {
            MsgId = Guid.NewGuid().ToString(),
            Verb = Message.Types.Verb.Set,
            Path = "/api/v1/integration/activate",
            Origin = "backend",
            Result = new Result { Code = Result.Types.Code.Success },
            Payload = Any.Pack(payload, "type.googleapis.com/logi.protocol.integrations.InstanceInfo"),
        };
        var res = activatedResponse.ToString();
        await SendAsync(context, activatedResponse.ToByteArray());
        Logger.Info("Sent activated response");
    }

    private async Task SendRegisteredResponse(IWebSocketContext context, Any? payload)
    {
        var registeredResponse = new Message()
        {
            MsgId = Guid.NewGuid().ToString(),
            Verb = Message.Types.Verb.Set,
            Path = "/api/v1/integration/register",
            Origin = "backend",
            Result = new Result { Code = Result.Types.Code.Success },
            Payload = payload,
        };
        var res = registeredResponse.ToString();
        await SendAsync(context, registeredResponse.ToByteArray());
        Logger.Info("Sent registered response");
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
        
        //Console.WriteLine($"Sending init message {initMessage}");

       // await context.WebSocket.SendAsync(initMessage.ToByteArray(), false);

        await base.OnClientConnectedAsync(context);
    }
}