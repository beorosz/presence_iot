using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var appConfig = LoadAppSettings();

            if (appConfig == null)
            {
                Console.WriteLine("Missing or invalid appsettings.json...exiting");
                return;
            }

            var appId = appConfig["appId"];
            var scopesString = appConfig["scopes"];
            var tenantId = appConfig["tenantId"];
            // var clientSecret = appConfig["clientSecret"];
            var scopes = scopesString.Split(';');

            try
            {
                var authProvider = new DeviceCodeAuthProvider(appId, tenantId, scopes);
                var accessToken = await authProvider.GetAccessToken();

                GraphHelper.Initialize(authProvider);

                while (true)
                {
                    var presence = await GraphHelper.GetMyPresenceAsync();
                    Console.WriteLine($"{DateTime.Now.ToString("s")} - {presence.Activity}");
                    Thread.Sleep(30000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static IConfigurationRoot LoadAppSettings()
        {
            var appConfig = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Check for required settings
            if (string.IsNullOrEmpty(appConfig["appId"]) ||
                string.IsNullOrEmpty(appConfig["tenantId"]) ||
                //string.IsNullOrEmpty(appConfig["clientSecret"]) ||
                string.IsNullOrEmpty(appConfig["scopes"]))
            {
                return null;
            }

            return appConfig;
        }
    }
}
