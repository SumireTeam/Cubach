using Cubach.Shared;
using System.Globalization;
using System.IO;

namespace Cubach.Server
{
    public static class Program
    {
        private static Configuration config;

        private static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            const string configPath = "./config.json";
            var configData = File.ReadAllText(configPath);
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configData);

            int port = args.Length > 0 ? int.Parse(args[0]) : 7800;

            using (var server = new Server(config))
            {
                server.Run(port);
            }
        }
    }
}
