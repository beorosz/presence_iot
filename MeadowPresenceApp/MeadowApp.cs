using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
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
            logger = new Logger(hardwareElements);
            rgbLedActions = new RgbLedActions(hardwareElements.RgbLed);
        }

        public void Initialize()
        {
            var tokenProvider = new AccessTokenProvider(configuration, logger);
            presenceProvider = new PresenceProvider(tokenProvider, logger);

            ntpClient = new NtpClient(configuration);
            var currentTime = ntpClient.GetUtcNetworkTime();
            Device.SetClock(currentTime);
        }

        public async Task RunMainLoop()
        {
            rgbLedActions.LightsOff();

            while (true)
            {
                try
                {
                    logger.Log(Category.Information, "Getting presence");
                    var presence = await presenceProvider.GetPresence();

                    var time = DateTime.Now.ToString("HH:mm");
                    logger.Log(Category.Presence, $"{time} {presence.activity}");

                    var ledAction = GetLedActionBy(presence.activity);
                    ledAction();

                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    logger.Log(Category.Error, e.Message);
                }
            }
        }

        private HardwareElements InitializeHardware()
        {
            byte LCD1602I2CAddress = 0x27;

            var rgbLed = new RgbLed(Device.CreateDigitalOutputPort(Device.Pins.D02),
                Device.CreateDigitalOutputPort(Device.Pins.D03),
                Device.CreateDigitalOutputPort(Device.Pins.D04), Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            rgbLed.Stop();

            II2cBus i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            var lcd1602 = new I2cCharacterDisplay(i2cBus, LCD1602I2CAddress, 2, 16);

            Device.InitWiFiAdapter().Wait();
            if (Device.WiFiAdapter.Connect(configuration["wifi_ssid"], configuration["wifi_password"])
                .Result.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception("Cannot connect to network, application halted.");
            }

            return new HardwareElements(rgbLed, lcd1602);
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
    }
}