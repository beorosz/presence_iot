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

        public async Task Run()
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

            lightActions = new LightActions(controller, redPin, greenPin, bluePin);

            Console.CancelKeyPress += delegate
            {
                new Task(() => lightActions.LightsOffAction(tokenSource.Token)).RunSynchronously();
                tokenSource.Dispose();
            };

            tokenSource = new CancellationTokenSource();
            var currentTask = Task.Run(() => lightActions.LightsOffAction(tokenSource.Token), tokenSource.Token);

            while (true)
            {
                try
                {
                    var presence = await GraphHelper.GetMyPresenceAsync();
                    Console.WriteLine($"{DateTime.Now.ToString("s")} - {presence.Activity}");

                    tokenSource.Cancel();
                    try
                    {
                        await currentTask;    
                    }
                    catch (System.Exception) { }
                    
                    var action = this.GetLightActionBy(presence.Activity);
                    tokenSource = new CancellationTokenSource();
                    currentTask = Task.Run(() => action(tokenSource.Token), tokenSource.Token);

                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }                
            }
        }       

        Action<CancellationToken> GetLightActionBy(string presenceActivity)
        {
            switch (presenceActivity)
            {
                case "Available":
                case "InAMeeting":
                    return lightActions.GreenLightOnAction;
                case "InACall":
                case "InAConferenceCall":
                case "Busy":
                case "DoNotDisturb":
                case "Presenting":
                case "UrgentInterruptionsOnly":
                    return lightActions.RedLightBlinkerAction;
                case "Away":
                case "BeRightBack":
                case "Inactive":
                    return lightActions.YellowLightOnAction;
                case "PresenceUnknown":
                case "Offline":
                case "OffWork":
                case "OutOfOffice":
                default:
                    return lightActions.LightsOffAction;
            }
        }
    }
}