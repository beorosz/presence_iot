using Meadow.Gateway.WiFi;
using Microsoft.Extensions.Configuration;
using System;

namespace MeadowPresenceApp
{
    public class WifiAdapter
    {
        private readonly IConfiguration configuration;

        public WifiAdapter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Init()
        {
            MeadowApp.Device.InitWiFiAdapter().Wait();

            if (MeadowApp.Device.WiFiAdapter.Connect(configuration["wifi_ssid"], configuration["wifi_password"])
                .Result.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception("Cannot connect to network, application halted.");
            }
        }
    }
}
