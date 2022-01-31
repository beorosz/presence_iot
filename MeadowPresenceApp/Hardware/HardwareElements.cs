﻿using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.Leds;

namespace MeadowPresenceApp.Hardware
{
    public class HardwareElements
    {
        public RgbLed RgbLed { get; }
        public I2cCharacterDisplay Lcd1602 { get; }
        public TftDisplay TftDisplay { get; }

        public HardwareElements(RgbLed rgbLed, I2cCharacterDisplay lcd1602, TftDisplay tftDisplay)
        {
            RgbLed = rgbLed;
            Lcd1602 = lcd1602;
            TftDisplay = tftDisplay;
        }
    }
}
