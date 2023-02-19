using System.Net;
using System.Net.Sockets;
using System.Text;
using EmbedIO;
using EmbedIO.WebSockets;
using Logi.Protocol;
using Logi.Protocol.Integrations;

namespace LogitechSdkListener
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new WebServer(9010);
            server.WithModule(new LogitechWebSocketModule());
            await server.RunAsync();
        }
    }
    
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
                var x = context;
                var a = result;
                var instanceInfo = InstanceInfo.Parser.ParseFrom(buffer);
                var sdkIntegration = SDKIntegration.Parser.ParseFrom(buffer);
                var istate = IntegrationStates.Types.IntegrationState.Parser.ParseFrom(buffer);
                var thingy = Integration.Types.Action.Types.Register.Parser.ParseFrom(buffer);
                Console.WriteLine(thingy);
                //var build string with buffer as hex
                var sb = new StringBuilder();
                foreach (var b in buffer)
                {
                    sb.Append(b.ToString("X2"));
                    sb.Append(" ");
                }
                Console.WriteLine(sb.ToString());
            });

            
            return Task.CompletedTask;
        }

        protected override Task OnClientConnectedAsync(IWebSocketContext context)
        {
            return base.OnClientConnectedAsync(context);
        }

        protected override void OnStart(CancellationToken cancellationToken)
        {
            base.OnStart(cancellationToken);
        }

        protected override Task OnFrameReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            return base.OnFrameReceivedAsync(context, buffer, result);
        }
    }
}