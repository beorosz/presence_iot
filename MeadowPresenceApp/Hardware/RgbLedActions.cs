using Meadow.Foundation.Leds;
using System;

namespace MeadowPresenceApp.Hardware
{
    public interface IRgbLedActions
    {
        Action RedLightBlinks { get; }
        Action GreenLightOn { get; }
        Action YellowLightOn { get; }
        Action BlueLightOn { get; }
        Action LightsOff { get; }
    }
    public class RgbLedActions : IRgbLedActions
    {
        private RgbLed rgbLed;

        public RgbLedActions(RgbLed rgbLed)
        {
            this.rgbLed = rgbLed;
        }

        public Action RedLightBlinks
        {
            get
            {
                return () =>
                {
                    rgbLed.Stop();
                    rgbLed.StartBlink(RgbLed.Colors.Red, 500, 500);
                };
            }
        }

        public Action GreenLightOn
        {
            get
            {
                return () =>
                {
                    rgbLed.Stop();
                    rgbLed.SetColor(RgbLed.Colors.Green);
                };
            }
        }

        public Action YellowLightOn
        {
            get
            {
                return () =>
                {
                    rgbLed.Stop();
                    rgbLed.SetColor(RgbLed.Colors.Yellow);
                };
            }
        }

        public Action BlueLightOn
        {
            get
            {
                return () =>
                {
                    rgbLed.Stop();
                    rgbLed.SetColor(RgbLed.Colors.Blue);
                };
            }
        }

        public Action LightsOff
        {
            get
            {
                return () =>
                {
                    rgbLed.Stop();
                };
            }
        }
    }
}