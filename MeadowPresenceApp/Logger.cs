using MeadowPresenceApp.Hardware;
using MeadowPresenceApp.Model;
using System;

namespace MeadowPresenceApp
{
    public interface ILogger
    {
        void Log(Category category, string message);
    }
    public class Logger : ILogger
    {
        private readonly HardwareElements hardwareElements;

        public Logger(HardwareElements hardwareElements)
        {
            this.hardwareElements = hardwareElements;
        }

        public void Log(Category category, string message)
        {
            switch(category)
            {
                case Category.Information:
                    LogInformation(message);
                    break;
                case Category.Error:
                    LogError(message);
                    break;
                case Category.DeviceCode:
                case Category.Presence:
                    LogImportantInfo(message);
                    break;
            }
        }

        private void LogImportantInfo(string message)
        {
            hardwareElements.Lcd1602.WriteLine(message, 1);
            Console.WriteLine(message);
        }

        private void LogError(string message)
        {
            hardwareElements.Lcd1602.WriteLine(message, 0);
            Console.WriteLine(message);
        }

        private void LogInformation(string message)
        {
            hardwareElements.Lcd1602.WriteLine(message, 0);
            Console.WriteLine(message);
        }
    }
}
