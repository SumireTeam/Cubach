using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Cubach
{
    public class Program : IDisposable
    {
        private readonly Server server;
        private readonly Client client;

        private Thread serverThread;

        private Program()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            const string configPath = "./config.json";
            var configData = File.ReadAllText(configPath);
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);
            client = new Client(config);
            server = new Server(config);
        }

        private void Run()
        {
            serverThread = new Thread(() => { server.Run(); });
            serverThread.Start();

            var clientMessageQueue = new Queue<IMessage>();
            var serverMessageQueue = new Queue<IMessage>();

            var serverConnection = new LocalServerConnection(clientMessageQueue, serverMessageQueue);
            client.Connect(serverConnection);

            var clientConnection = new LocalClientConnection(serverMessageQueue, clientMessageQueue);
            server.Connect(clientConnection);

            client.Run();
        }

        public void Dispose()
        {
            serverThread?.Abort();

            server.Dispose();
            client.Dispose();
        }

        private static void Main()
        {
            using (var program = new Program()) {
                program.Run();
            }
        }
    }
}
