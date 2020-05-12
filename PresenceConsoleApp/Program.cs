using System;
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
            var clientSecret = appConfig["clientSecret"];
            var scopes = scopesString.Split(';');

            //var authProvider = new DeviceCodeAuthProvider(appId, tenantId, scopes);
            //var accessToken = await authProvider.GetAccessToken();

            var authProvider = new MyAuthProvider(appId, tenantId, clientSecret);
            var accessToken = await authProvider.GetAccessToken();

            GraphHelper.Initialize(authProvider);

            //var user = await GraphHelper.GetMeAsync();
            //Console.WriteLine($"Welcome {user.DisplayName}!\n");
            //var presence = GraphHelper.GetPresence(accessToken).Result;
            var presence = await GraphHelper.GetMyPresenceAsync();
            Console.WriteLine(presence.Availability);
            
        }

        static IConfigurationRoot LoadAppSettings()
        {
            var appConfig = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            // Check for required settings
            if (string.IsNullOrEmpty(appConfig["appId"]) ||
                string.IsNullOrEmpty(appConfig["tenantId"]) ||
                string.IsNullOrEmpty(appConfig["clientSecret"]) ||
                string.IsNullOrEmpty(appConfig["scopes"]))
            {
                return null;
            }

            return appConfig;
        }
    }
}
