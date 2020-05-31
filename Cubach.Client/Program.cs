using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Cubach.Client.Network;
using Cubach.Shared;

namespace Cubach.Client
{
    public static class Program
    {
        private static Configuration config;

        private static void RunLocalGame()
        {
            using (var client = new Client(config))
            {
                var serverQueue = new Queue<ServerMessage>();
                var clientQueue = new Queue<ClientMessage>();

                var serverThread = new Thread(() =>
                {
                    using (var server = new LocalServer(config))
                    {
                        server.Run(serverQueue, clientQueue);
                    }
                });

                serverThread.Start();
                client.ConnectLocal(serverQueue, clientQueue);
                serverThread.Abort();
            }
        }

        private static void RunClient(string host, int port)
        {
            using (var client = new Client(config))
            {
                client.ConnectRemote(host, port);
            }
        }

        private static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            const string configPath = "./config.json";
            var configData = File.ReadAllText(configPath);
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);

            if (args.Length == 3 && args[0] == "-c")
            {
                RunClient(args[1], int.Parse(args[2]));
            }
            else
            {
                RunLocalGame();
            }
        }
    }
}
