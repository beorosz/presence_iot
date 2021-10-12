using MeadowPresenceApp.Hardware;
using MeadowPresenceApp.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MeadowPresenceApp
{
    public interface ILogger
    {
        void Log(LogMessage message);
        void LogDeviceCode(string deviceCode);
        void LogPresence(string presence);
    }
    public class Logger : ILogger
    {
        private readonly HardwareElements hardwareElements;
        private readonly IConfiguration configuration;

        private Queue<LogMessage> logMessages;
        private int logMessageLimit;

        private string flowInfoMessage;

        public Logger(HardwareElements hardwareElements, IConfiguration configuration)
        {
            this.hardwareElements = hardwareElements;
            this.configuration = configuration;
            InitMessageStore();
        }

        public void Log(LogMessage logMessage)
        {
            if (logMessage.Category == Category.Debug && bool.Parse(configuration["DebugLogEnabled"]) == true)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {logMessage.Message}");

                return;
            }
            StoreLogMessage(logMessage);

            hardwareElements.TftDisplay.Render(logMessages, flowInfoMessage);
        }

        public void LogDeviceCode(string deviceCode)
        {
            flowInfoMessage = deviceCode;
            hardwareElements.TftDisplay.Render(logMessages, flowInfoMessage);
        }

        public void LogPresence(string presence)
        {
            flowInfoMessage = presence;
            hardwareElements.TftDisplay.Render(logMessages, flowInfoMessage);
        }

        private void InitMessageStore()
        {
            logMessages = new Queue<LogMessage>();
            logMessageLimit = 6;
            flowInfoMessage = string.Empty;
        }
        private void StoreLogMessage(LogMessage message)
        {
            logMessages.Enqueue(message);
            if (logMessages.Count > logMessageLimit)
            {
                logMessages.Dequeue();
            }
        }
    }
}
