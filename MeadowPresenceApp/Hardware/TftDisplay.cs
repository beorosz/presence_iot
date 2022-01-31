using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using MeadowPresenceApp.Model;
using System;
using System.Collections.Generic;

namespace MeadowPresenceApp.Hardware
{
    public class TftDisplay
    {
        private GraphicsLibrary graphics;
        private St7789 display;

        private object renderLock = new object();
        private bool isRendering = false;

        private Font6x8 logFont = new Font6x8();
        private Font12x20 flowInfoFont = new Font12x20();
        private Font8x12 timeFont = new Font8x12();

        public TftDisplay(GraphicsLibrary graphics, St7789 display)
        {
            this.graphics = graphics;
            this.display = display;
        }

        public static TftDisplay Initialize()
        {
            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);
            var display = new St7789
            (
                device: MeadowApp.Device,
                spiBus: MeadowApp.Device.CreateSpiBus(
                    MeadowApp.Device.Pins.SCK,
                    MeadowApp.Device.Pins.MOSI,
                    MeadowApp.Device.Pins.MISO,
                    config),
                chipSelectPin: null,
                dcPin: MeadowApp.Device.Pins.D01,
                resetPin: MeadowApp.Device.Pins.D00,
                width: 240, height: 240
            );

            var graphics = new GraphicsLibrary(display);

            graphics.Clear(true);

            return new TftDisplay(graphics, display);
        }

        public void Render(Queue<LogMessage> logMessages, string flowMessage)
        {
            lock (renderLock)
            {
                if (isRendering)
                {
                    Console.WriteLine("Already in a rendering loop, bailing out.");
                    return;
                }

                isRendering = true;
            }

            graphics.Clear(false);

            DrawLogMessages(logMessages);
            DrawFlowMessage(flowMessage);
            DrawTime();

            graphics.Show();

            isRendering = false;
        }

        private void DrawLogMessages(Queue<LogMessage> logMessages)
        {
            int i = 0;
            int basePosition = 10;

            foreach (var logMessage in logMessages)
            {
                graphics.CurrentFont = logFont;
                graphics.DrawText(
                    x: 0,
                    y: basePosition + i * (graphics.CurrentFont.Height + 10),
                    text: logMessage.Message,
                    color: GetFontColorBy(logMessage.Category),
                    scaleFactor: GraphicsLibrary.ScaleFactor.X2);

                i++;
            }
        }

        private void DrawFlowMessage(string flowInfoMessage)
        {
            graphics.CurrentFont = flowInfoFont;
            graphics.DrawText(
                x: 0,
                y: 150,
                text: flowInfoMessage,
                color: GetFontColorBy(Category.Flow),
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);
        }

        private void DrawTime()
        {
            var timeString = DateTime.Now.ToString("HH:mm");
            graphics.CurrentFont = timeFont;
            graphics.DrawText(
                    x: (display.Width - timeString.Length * 20) / 2,
                    y: 220,
                    text: timeString,
                    color: Color.Aqua,
                    scaleFactor: GraphicsLibrary.ScaleFactor.X2);
        }

        private Color GetFontColorBy(Category category)
        {
            switch (category)
            {
                case Category.Information:
                    return Color.Green;
                case Category.Error:
                    return Color.Red;
                case Category.Flow:
                    return Color.White;
                default:
                    return Color.Yellow;
            }
        }        
    }
}
