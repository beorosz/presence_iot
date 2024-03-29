﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp {
  class Program {
    static async Task Main (string[] args) {
      var appConfig = LoadAppSettings ();

      if (appConfig == null) {
        Console.WriteLine ("Missing or invalid application settings...exiting.");
        return;
      }

      var app = new PresenceConsoleApp (appConfig);
      await app.Run ();
    }

    static IConfigurationRoot LoadAppSettings () {
      var appConfig = new ConfigurationBuilder ()
        .AddEnvironmentVariables ()
        .Build ();

      // Check for required settings
      if (
        string.IsNullOrEmpty (appConfig["APPID"]) ||
        string.IsNullOrEmpty (appConfig["TENANTID"]) ||
        string.IsNullOrEmpty (appConfig["SCOPES"]) ||
        string.IsNullOrEmpty (appConfig["LED_RED_PIN"]) ||
        string.IsNullOrEmpty (appConfig["LED_GREEN_PIN"]) ||
        string.IsNullOrEmpty (appConfig["LED_BLUE_PIN"])
      ) {
        return null;
      }

      return appConfig;
    }
  }
}