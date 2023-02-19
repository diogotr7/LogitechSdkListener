using EmbedIO;
using Logi.Protocol;

namespace LogitechSdkListener
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //DissectBytes();
            //return;
            
            var server = new WebServer(9010);
            server.WithModule(new LogitechWebSocketModule());
            await server.RunAsync();
        }

        private static void DissectBytes()
        {
            //get all files in "dumps" folder
            foreach (var file in Directory.GetFiles("dumps", "*.bin"))
            {
                var jsonFile = file.Replace(".bin", ".json");
                var payloadFile = file.Replace(".bin", ".payload.json");
                if (File.Exists(jsonFile) && File.Exists(payloadFile))
                {
                    continue;
                }
                try
                {
                    var bytes = File.ReadAllBytes(file);
                    var message = Message.Parser.ParseFrom(bytes);
                    if (message.Payload is not null)
                    {
                        //get message data
                        var messageData = message.Payload.Unpack(LogitechSdkTypeRegistry.TypeRegistry);
                        //print message data
                        Console.WriteLine(messageData);
                        File.WriteAllText(payloadFile, messageData.ToString());
                    }

                    File.WriteAllText(jsonFile, message.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}