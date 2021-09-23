using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowPresenceApp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug") return;

            var configuration = GetConfigurationRoot();
            var app = new MeadowApp(configuration);
            
            app.Initialize();
            await app.RunMainLoop();

            Thread.Sleep(Timeout.Infinite);
        }

        private static IConfigurationRoot GetConfigurationRoot()
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("config.json")
               .Build();

            return configuration;
        }
    }
}
