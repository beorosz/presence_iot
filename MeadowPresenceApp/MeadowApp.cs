using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
using Meadow.Foundation.Leds;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MeadowPresenceApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly IConfiguration configuration;
        private readonly IPresenceProvider presenceProvider;
        private IRgbLedActions rgbLedActions;
        private RgbLed rgbLed;
        private I2cCharacterDisplay lcd1602;

        public MeadowApp(IConfiguration configuration)
        {
            this.configuration = configuration;
            var tokenProvider = new AccessTokenProvider(configuration, AccessTokenProviderNotificationCallback, AccessTokenProviderDeviceCodeCallback);
            presenceProvider = new PresenceProvider(tokenProvider);
        }

        public void Initialize()
        {
            InitializeHardware();
            rgbLedActions = new RgbLedActions(rgbLed);
        }

        public async Task RunMainLoop()
        {
            rgbLedActions.LightsOff();

            while (true)
            {
                try
                {
                    var presence = await presenceProvider.GetPresence();
                    lcd1602.WriteLine($"{presence.activity}", 1);

                    var ledAction = GetLedActionBy(presence.activity);
                    ledAction();

                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(e.InnerException);
                }
            }
        }

        private void InitializeHardware()
        {
            byte LCD1602I2CAddress = 0x27;

            new WifiAdapter(configuration).Init();

            rgbLed = new RgbLed(Device.CreateDigitalOutputPort(Device.Pins.D02),
                Device.CreateDigitalOutputPort(Device.Pins.D03),
                Device.CreateDigitalOutputPort(Device.Pins.D04), Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            rgbLed.Stop();

            II2cBus i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            lcd1602 = new I2cCharacterDisplay(i2cBus, LCD1602I2CAddress, 2, 16);
        }

        private Action GetLedActionBy(string presenceActivity)
        {
            var date = DateTime.Now;
            //if (date.Hour >= 20 || date.Hour < 8)
            //{
            //    return lightActions.LightsOffAction;
            //}

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

        private void AccessTokenProviderNotificationCallback(string message)
        {
            lcd1602.WriteLine(message, 0);
        }

        private void AccessTokenProviderDeviceCodeCallback(string message)
        {
            lcd1602.WriteLine(message, 1);
        }
    }
}