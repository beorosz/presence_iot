using LitJson;
using MeadowPresenceApp.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowPresenceApp
{
    public interface IAccessTokenProvider
    {
        Task<string> GetAccessToken();
    }

    public class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IConfiguration configuration;
        private string accessToken;

        public Action<string> NotificationCallback { get; }
        public Action<string> DisplayDeviceCodeCallback { get; }

        public AccessTokenProvider(IConfiguration configuration, Action<string> notificationCallback, Action<string> displayDeviceCodeCallback)
        {
            this.configuration = configuration;
            
            NotificationCallback = notificationCallback;
            DisplayDeviceCodeCallback = displayDeviceCodeCallback;

            accessToken = string.Empty;
        }

        public async Task<string> GetAccessToken()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                var deviceCodeResponse = await GetDeviceCodeResponse();
                var accessTokenResponse = await GetAccessToken(deviceCodeResponse.interval, deviceCodeResponse.device_code);

                accessToken = accessTokenResponse.access_token;
            }

            return accessToken;
        }

        private async Task<GetDeviceCodeResponse> GetDeviceCodeResponse()
        {
            GetDeviceCodeResponse result = null;

            var appId = configuration["appId"];
            var tenantId = configuration["tenantId"];
            var scopes = configuration["scopes"];

            var requestContent = new Dictionary<string, string>();
            requestContent.Add("client_id", appId);
            requestContent.Add("scope", scopes);
            var requestUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/devicecode";

            try
            {
                NotificationCallback("Get device code");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 30);
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = new FormUrlEncodedContent(requestContent)
                    };
                    var httpResponse = await client.SendAsync(request);                    
                    httpResponse.EnsureSuccessStatusCode();

                    var responseString = await httpResponse.Content.ReadAsStringAsync();
                    result = JsonMapper.ToObject<GetDeviceCodeResponse>(responseString);
                    DisplayDeviceCodeCallback($"{result.user_code}");
                }
            }
            catch (Exception e)
            {
                NotificationCallback(e.Message);
                throw;
            }

            return result;
        }

        private async Task<GetAccessTokenResponse> GetAccessToken(int pollingInterval, string deviceCode)
        {
            GetAccessTokenResponse result = new GetAccessTokenResponse();

            var appId = configuration["appId"];
            var tenantId = configuration["tenantId"];

            var continuePolling = true;
            var pollingIntervalInSecs = pollingInterval;

            var requestContent = new Dictionary<string, string>();
            requestContent.Add("grant_type", "urn:ietf:params:oauth:grant-type:device_code");
            requestContent.Add("client_id", appId);
            requestContent.Add("device_code", deviceCode);
            var requestUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            while (continuePolling)
            {
                NotificationCallback("Waiting for auth");
                Thread.Sleep(pollingIntervalInSecs * 1000);

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 30);
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = new FormUrlEncodedContent(requestContent)
                    };
                    var httpResponse = await client.SendAsync(request);
                    var responseString = await httpResponse.Content.ReadAsStringAsync();

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        continuePolling = false;
                        result = JsonMapper.ToObject<GetAccessTokenResponse>(responseString);
                        NotificationCallback($"Token acquired");
                    }
                    else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorResponse = JsonMapper.ToObject<BadRequestResponse>(responseString);
                        if (errorResponse.error == "authorization_pending")
                        {
                            continue;
                        }
                        else
                        {
                            NotificationCallback(errorResponse.error);
                            throw new AuthorizationException(errorResponse.error);
                        }
                    }
                }
            }

            return result;
        }
    }
}
