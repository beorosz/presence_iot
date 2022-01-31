using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MeadowPresenceApp.Hardware;
using Meadow.Gateway.WiFi;
using MeadowPresenceApp.Communication;
using MeadowPresenceApp.Model;

namespace MeadowPresenceApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private IPresenceProvider presenceProvider;
        private NtpClient ntpClient;

        private IRgbLedActions rgbLedActions;
        private HardwareElements hardwareElements;

        public MeadowApp(IConfiguration configuration)
        {
            this.configuration = configuration;
            hardwareElements = InitializeHardware();
            logger = new Logger(hardwareElements, configuration);
            rgbLedActions = new RgbLedActions(hardwareElements.RgbLed);
        }

        public void Initialize()
        {
            var tokenProvider = new AccessTokenProvider(configuration, logger);
            presenceProvider = new PresenceProvider(tokenProvider, logger);

            ntpClient = new NtpClient(configuration);
            var currentTime = ntpClient.GetUtcNetworkTime();
            Device.SetClock(currentTime);

            MeadowOS.WatchdogEnable(10000);
            StartPettingWatchdog(9000);
        }

        public async Task RunMainLoop()
        {
            rgbLedActions.LightsOff();

            while (true)
            {
                try
                {
                    logger.Log(new LogMessage(Category.Debug, "Getting presence"));
                    var presence = await presenceProvider.GetPresence();
                    logger.Log(new LogMessage(Category.Debug, "Presence retrieved"));

                    logger.LogPresence(presence.activity);

                    var ledAction = GetLedActionBy(presence.activity);
                    ledAction();

                    logger.Log(new LogMessage(Category.Debug, "Waiting for 10 seconds"));
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    logger.Log(new LogMessage(Category.Error, e.Message));
                    logger.Log(new LogMessage(Category.Debug, e.Message));
                    logger.Log(new LogMessage(Category.Debug, e.StackTrace));
                }
            }
        }

        private HardwareElements InitializeHardware()
        {
            var redPin = Device.CreateDigitalOutputPort(Device.Pins.D02, true);
            var greenPin = Device.CreateDigitalOutputPort(Device.Pins.D03, true);
            var bluePin = Device.CreateDigitalOutputPort(Device.Pins.D04, true);
            var rgbLed = new RgbLed(redPin, greenPin, bluePin, Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            rgbLed.Stop();

            var tftDisplay = TftDisplay.Initialize();

            Device.InitWiFiAdapter().Wait();
            if (Device.WiFiAdapter.Connect(configuration["wifi_ssid"], configuration["wifi_password"])
                .Result.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception("Cannot connect to network, application halted.");
            }

            return new HardwareElements(rgbLed, null, tftDisplay);
        }

        private Action GetLedActionBy(string presenceActivity)
        {
            var date = DateTime.Now;
            if (date.Hour >= 20 || date.Hour < 8)
            {
                return rgbLedActions.LightsOff;
            }

            switch (presenceActivity)
            {
                case "Available":
                    return rgbLedActions.GreenLightOn;
                case "InAMeeting":
                case "InACall":
                case "InAConferenceCall":
                case "Busy":
                case "DoNotDisturb":
                case "Presenting":
                case "UrgentInterruptionsOnly":
                    return rgbLedActions.RedLightBlinks;
                case "Away":
                case "BeRightBack":
                case "Inactive":
                    return rgbLedActions.BlueLightOn;
                case "PresenceUnknown":
                case "Offline":
                case "OffWork":
                case "OutOfOffice":
                default:
                    return rgbLedActions.LightsOff;
            }
        }

        private void StartPettingWatchdog(int pettingInterval)
        {
            MeadowOS.WatchdogReset();
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(pettingInterval);
                    MeadowOS.WatchdogReset();
                }
            });
            t.Start();
        }
    }
}