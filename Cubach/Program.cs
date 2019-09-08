using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Cubach.Network.Local;

namespace Cubach
{
    public static class Program
    {
        private static Configuration config;

        private static void RunLocalGame()
        {
            using (var client = new Client(config)) {
                var serverQueue = new Queue<ServerMessage>();
                var clientQueue = new Queue<ClientMessage>();

                var serverThread = new Thread(() => {
                    using (var server = new Server(config)) {
                        server.RunLocal(serverQueue, clientQueue);
                    }
                });

                serverThread.Start();
                client.ConnectLocal(serverQueue, clientQueue);
                serverThread.Abort();
            }
        }

        private static void RunServer(int port)
        {
            using (var server = new Server(config)) {
                server.RunRemote(port);
            }
        }

        private static void RunClient(string host, int port)
        {
            using (var client = new Client(config)) {
                client.ConnectRemote(host, port);
            }
        }

        private static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            const string configPath = "./config.json";
            var configData = File.ReadAllText(configPath);
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);

            // TODO: implement argument parser.

            if (args.Length == 2 && args[0] == "-s") {
                RunServer(int.Parse(args[1]));
            }
            else if (args.Length == 3 && args[0] == "-c") {
                RunClient(args[1], int.Parse(args[2]));
            }
            else {
                RunLocalGame();
            }
        }
    }
}
