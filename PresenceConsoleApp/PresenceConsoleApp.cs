using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PresenceConsoleApp
{
    public class PresenceConsoleApp
    {
        private IConfigurationRoot appConfig;
        private LightActions lightActions;

        private CancellationTokenSource tokenSource;
        
        int redPin = 19, greenPin = 26, bluePin = 13;

        public PresenceConsoleApp(IConfigurationRoot appConfig)
        {
            this.appConfig = appConfig;
        }

        public async void Run()
        {
            var appId = appConfig["appId"];
            var scopesString = appConfig["scopes"];
            var tenantId = appConfig["tenantId"];
            var scopes = scopesString.Split(';');
            var authProvider = new DeviceCodeAuthProvider(appId, tenantId, scopes);

            GraphHelper.Initialize(authProvider);

            GpioController controller = new GpioController();
            controller.OpenPin(redPin, PinMode.Output);
            controller.OpenPin(greenPin, PinMode.Output);
            controller.OpenPin(bluePin, PinMode.Output);

            lightActions = new LightActions(controller, tokenSource.Token, redPin, greenPin, bluePin);

            Console.CancelKeyPress += delegate
            {
                new Task(lightActions.LightsOffAction, tokenSource.Token).RunSynchronously();
                tokenSource.Dispose();
            };

            var currentTask = new Task(lightActions.LightsOffAction, tokenSource.Token);
            currentTask.RunSynchronously();

            while (true)
            {
                try
                {
                    var presence = await GraphHelper.GetMyPresenceAsync();
                    Console.WriteLine($"{DateTime.Now.ToString("s")} - {presence.Activity}");

                    tokenSource.Cancel();
                    await currentTask;
                    currentTask = this.GetLightTaskBy(presence.Activity);

                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    tokenSource.Dispose();
                }
            }
        }

        (bool turnOnRedPin, bool turnOnGreenPin, bool turnOnBluePin) GetPinFlagsFrom(string presenceActivity)
        {
            switch (presenceActivity)
            {
                case "Available":
                case "InAMeeting":
                    return (false, true, false); //green
                case "InACall":
                case "InAConferenceCall":
                case "Busy":
                case "DoNotDisturb":
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

        Task GetLightTaskBy(string presenceActivity)
        {
            switch (presenceActivity)
            {
                case "Available":
                case "InAMeeting":
                    return new Task(lightActions.GreenLightOnAction, tokenSource.Token);
                case "InACall":
                case "InAConferenceCall":
                case "Busy":
                case "DoNotDisturb":
                case "Presenting":
                case "UrgentInterruptionsOnly":
                    return new Task(lightActions.RedLightBlinkerAction, tokenSource.Token);
                case "Away":
                case "BeRightBack":
                case "Inactive":
                    return new Task(lightActions.YellowLightOnAction, tokenSource.Token);
                case "PresenceUnknown":
                case "Offline":
                case "OffWork":
                case "OutOfOffice":
                default:
                    return new Task(lightActions.LightsOffAction, tokenSource.Token);
            }
        }
    }
}