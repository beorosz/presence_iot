using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp {
  class Program {
    static async Task Main (string[] args) {
      var appConfig = LoadAppSettings ();

      if (appConfig == null) {
        Console.WriteLine ("Missing or invalid appsettings.json...exiting");
        return;
      }

      var app = new PresenceConsoleApp (appConfig);
      await app.Run ();
    }

    static IConfigurationRoot LoadAppSettings () {
      var appConfig = new ConfigurationBuilder ()
        .AddUserSecrets<Program> ()
        .Build ();

      // Check for required settings
      if (string.IsNullOrEmpty (appConfig["appId"]) ||
        string.IsNullOrEmpty (appConfig["tenantId"]) ||
        string.IsNullOrEmpty (appConfig["scopes"])) {
        return null;
      }

      return appConfig;
    }
  }
}