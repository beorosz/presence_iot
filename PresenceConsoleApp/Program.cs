using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp {
  class Program {
    static async Task Main (string[] args) {
      int redPin = 19, greenPin = 26, bluePin = 13;

      var appConfig = LoadAppSettings ();

      if (appConfig == null) {
        Console.WriteLine ("Missing or invalid appsettings.json...exiting");
        return;
      }

      var appId = appConfig["appId"];
      var scopesString = appConfig["scopes"];
      var tenantId = appConfig["tenantId"];
      // var clientSecret = appConfig["clientSecret"];
      var scopes = scopesString.Split (';');
      GpioController controller = new GpioController ();

      Console.CancelKeyPress += delegate {
        controller.Write (redPin, PinValue.High);
        controller.Write (greenPin, PinValue.High);
        controller.Write (bluePin, PinValue.High);
      };

      try {
        var authProvider = new DeviceCodeAuthProvider (appId, tenantId, scopes);
        var accessToken = await authProvider.GetAccessToken ();

        GraphHelper.Initialize (authProvider);

        controller.OpenPin (redPin, PinMode.Output);
        controller.OpenPin (greenPin, PinMode.Output);
        controller.OpenPin (bluePin, PinMode.Output);

        while (true) {
          var presence = await GraphHelper.GetMyPresenceAsync ();
          var pinFlags = GetPinFlagsFrom (presence.Activity);
          Console.WriteLine ($"{DateTime.Now.ToString("s")} - {presence.Activity}");
          controller.Write (redPin, pinFlags.turnOnRedPin ? PinValue.Low : PinValue.High);
          controller.Write (greenPin, pinFlags.turnOnGreenPin ? PinValue.Low : PinValue.High);
          controller.Write (bluePin, pinFlags.turnOnBluePin ? PinValue.Low : PinValue.High);
          Thread.Sleep (10000);
        }
      } catch (Exception e) {
        Console.WriteLine (e);
      }
    }

    static IConfigurationRoot LoadAppSettings () {
      var appConfig = new ConfigurationBuilder ()
        .AddUserSecrets<Program> ()
        .Build ();

      // Check for required settings
      if (string.IsNullOrEmpty (appConfig["appId"]) ||
        string.IsNullOrEmpty (appConfig["tenantId"]) ||
        //string.IsNullOrEmpty(appConfig["clientSecret"]) ||
        string.IsNullOrEmpty (appConfig["scopes"])) {
        return null;
      }

      return appConfig;
    }

    static (bool turnOnRedPin, bool turnOnGreenPin, bool turnOnBluePin) GetPinFlagsFrom (string presenceActivity) {
      switch (presenceActivity) {
        case "Available":
          return (false, true, false); //green
        case "InACall":
        case "InAConferenceCall":
        case "Busy":
        case "DoNotDisturb":
        case "InAMeeting":
        case "Presenting":
        case "UrgentInterruptionsOnly":
          return (true, false, false); // red
        case "Away":
        case "BeRightBack":
        case "Inactive":
          return (true, true, false); // yellow        
        case "PresenceUnknown":
        case "Offline":
        case "OffWork":
        case "OutOfOffice":
        default:
          return (false, false, false); // light off
      }
    }
  }
}