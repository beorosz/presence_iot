using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp {
  public class PresenceConsoleApp {
    private IConfigurationRoot appConfig;
    private LightActions lightActions;

    private CancellationTokenSource tokenSource;

    int redPin, greenPin, bluePin;
    int ledOffBeforeHour = 8, ledOffAfterHour = 20;

    public PresenceConsoleApp (IConfigurationRoot appConfig) {
      this.appConfig = appConfig;

      ledOffBeforeHour = appConfig["LED_OFF_BEFORE_HOUR"] != null ? Convert.ToInt32 (appConfig["LED_OFF_BEFORE_HOUR"]) : ledOffBeforeHour;
      ledOffAfterHour = appConfig["LED_OFF_AFTER_HOUR"] != null ? Convert.ToInt32 (appConfig["LED_OFF_AFTER_HOUR"]) : ledOffBeforeHour;

      redPin = Convert.ToInt32 (appConfig["LED_RED_PIN"]);
      greenPin = Convert.ToInt32 (appConfig["LED_GREEN_PIN"]);
      bluePin = Convert.ToInt32 (appConfig["LED_BLUE_PIN"]);
    }

    private void AppExitHandler (object sender, EventArgs args) {
      new Task (() => lightActions.LightsOffAction (tokenSource.Token)).RunSynchronously ();
      tokenSource.Dispose ();
    }

    public async Task Run () {
      var appId = appConfig["APPID"];
      var scopesString = appConfig["SCOPES"];
      var tenantId = appConfig["TENANTID"];
      var scopes = scopesString.Split (';');
      var authProvider = new DeviceCodeAuthProvider (appId, tenantId, scopes);

      GraphHelper.Initialize (authProvider);

      GpioController controller = new GpioController ();
      controller.OpenPin (redPin, PinMode.Output);
      controller.OpenPin (greenPin, PinMode.Output);
      controller.OpenPin (bluePin, PinMode.Output);

      lightActions = new LightActions (controller, redPin, greenPin, bluePin);

      Console.CancelKeyPress += new ConsoleCancelEventHandler (AppExitHandler);
      AppDomain.CurrentDomain.ProcessExit += new EventHandler (AppExitHandler);

      tokenSource = new CancellationTokenSource ();
      var currentTask = Task.Run (() => lightActions.LightsOffAction (tokenSource.Token), tokenSource.Token);

      while (true) {
        try {
          var presence = await GraphHelper.GetMyPresenceAsync ();
          Console.WriteLine ($"{DateTime.Now.ToString("s")} - {presence?.Activity}");

          tokenSource.Cancel ();
          try {
            await currentTask;
          } catch (System.Exception) { }

          var action = this.GetLightActionBy (presence?.Activity);
          tokenSource = new CancellationTokenSource ();
          currentTask = Task.Run (() => action (tokenSource.Token), tokenSource.Token);

          Thread.Sleep (10000);
        } catch (Exception e) {
          Console.WriteLine (e);
          Console.WriteLine (e.InnerException);
        }
      }
    }

    Action<CancellationToken> GetLightActionBy (string presenceActivity) {
      var date = DateTime.Now;

      if (date.Hour < ledOffBeforeHour || date.Hour >= ledOffAfterHour) {
        return lightActions.LightsOffAction;
      }

      switch (presenceActivity) {
        case "Available":
          return lightActions.GreenLightOnAction;
        case "InAMeeting":
        case "InACall":
        case "InAConferenceCall":
        case "Busy":
        case "DoNotDisturb":
        case "Presenting":
        case "UrgentInterruptionsOnly":
          return lightActions.RedLightBlinksAction;
        case "Away":
        case "BeRightBack":
        case "Inactive":
          return lightActions.BlueLightOnAction;
        case "PresenceUnknown":
        case "Offline":
        case "OffWork":
        case "OutOfOffice":
          return lightActions.LightsOffAction;
        default:
          return lightActions.LightsOffAction;
      }
    }
  }
}