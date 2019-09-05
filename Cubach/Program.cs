using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Cubach
{
    public class Program : IDisposable
    {
        private readonly Server server;
        private readonly Client client;

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
            Task.Run(() => { server.Run(); });
            client.Connect(new LocalServerConnection(server));
            client.Run();
        }

        public void Dispose()
        {
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
