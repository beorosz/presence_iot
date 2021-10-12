using LitJson;
using MeadowPresenceApp.Communication;
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
        Task RefreshAccessToken();
    }

    public class AccessTokenProvider : HttpCommunicationBase, IAccessTokenProvider
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly string tokenRequestUri;
        private readonly string deviceCodeRequestUri;

        private string cachedAccessToken;
        private string cachedRefreshToken;

        public AccessTokenProvider(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            var tenantId = configuration["tenantId"];
            tokenRequestUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            deviceCodeRequestUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/devicecode";

            cachedAccessToken = cachedRefreshToken = string.Empty;
        }

        public async Task<string> GetAccessToken()
        {
            if (string.IsNullOrEmpty(cachedAccessToken))
            {
                logger.Log(new LogMessage(Category.Information, "Querying token"));
                var deviceCodeResponse = await GetDeviceCodeResponse();
                var accessTokenResponse = await PollForAccessToken(deviceCodeResponse.interval, deviceCodeResponse.device_code);

                cachedAccessToken = accessTokenResponse.access_token;
                cachedRefreshToken = accessTokenResponse.refresh_token;
            }

            return cachedAccessToken;
        }

        public async Task RefreshAccessToken()
        {
            var refreshTokenResponse = await RefreshAccessTokenInternal();

            cachedAccessToken = refreshTokenResponse.access_token;
            cachedRefreshToken = refreshTokenResponse.refresh_token;
        }

        private async Task<GetDeviceCodeResponse> GetDeviceCodeResponse()
        {
            GetDeviceCodeResponse result = null;

            var appId = configuration["appId"];
            var scopes = configuration["scopes"];

            var deviceCodeRequestContentDictionary = GetDeviceCodeRequestContentDictionary(appId, scopes);
            var deviceCodeRequestContent = new FormUrlEncodedContent(deviceCodeRequestContentDictionary);

            try
            {
                logger.Log(new LogMessage(Category.Information, "Getting dev.code"));
                var httpResponse = await HttpSend(HttpMethod.Post, deviceCodeRequestUri, deviceCodeRequestContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                logger.Log(new LogMessage(Category.Debug, $"{httpResponse.StatusCode} : {responseString}"));

                httpResponse.EnsureSuccessStatusCode();
                result = JsonMapper.ToObject<GetDeviceCodeResponse>(responseString);
                logger.LogDeviceCode(result.user_code);
            }
            catch (Exception e)
            {
                logger.Log(new LogMessage(Category.Error, e.Message));
                logger.Log(new LogMessage(Category.Debug, $"{e.Message}"));
                logger.Log(new LogMessage(Category.Debug, $"{e.StackTrace}"));
            }

            return result;
        }

        private async Task<GetAccessTokenResponse> PollForAccessToken(int pollingInterval, string deviceCode)
        {
            GetAccessTokenResponse result = new GetAccessTokenResponse();

            var continuePolling = true;
            var pollingIntervalInSecs = pollingInterval;
            var appId = configuration["appId"];

            var accessTokenRequestContentDictionary = GetAccessTokenRequestContentDictionary(appId, deviceCode);
            var accessTokenRequestContent = new FormUrlEncodedContent(accessTokenRequestContentDictionary);

            try
            {
                logger.Log(new LogMessage(Category.Information, "Waiting for auth"));
                while (continuePolling)
                {                    
                    Thread.Sleep(pollingIntervalInSecs * 1000);

                    var httpResponse = await HttpSend(HttpMethod.Post, tokenRequestUri, accessTokenRequestContent);
                    var responseString = await httpResponse.Content.ReadAsStringAsync();
                    logger.Log(new LogMessage(Category.Debug, $"{httpResponse.StatusCode} : {responseString}"));

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        continuePolling = false;
                        result = JsonMapper.ToObject<GetAccessTokenResponse>(responseString);
                        logger.Log(new LogMessage(Category.Information, "Token acquired"));
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
                            logger.Log(new LogMessage(Category.Error, errorResponse.error_description));
                            continuePolling = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                continuePolling = false;
                logger.Log(new LogMessage(Category.Error, e.Message));
                logger.Log(new LogMessage(Category.Debug, $"{e.Message}"));
                logger.Log(new LogMessage(Category.Debug, $"{e.StackTrace}"));
            }

            return result;
        }

        private async Task<GetAccessTokenResponse> RefreshAccessTokenInternal()
        {
            GetAccessTokenResponse result = new GetAccessTokenResponse();

            try
            {
                var appId = configuration["appId"];
                var refreshTokenRequestContent = GetRefreshTokenRequestContentDictionary(appId, cachedRefreshToken);

                var httpResponse = await HttpSend(HttpMethod.Post, tokenRequestUri, new FormUrlEncodedContent(refreshTokenRequestContent));
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                logger.Log(new LogMessage(Category.Debug, $"{httpResponse.StatusCode} : {responseString}"));

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = JsonMapper.ToObject<GetAccessTokenResponse>(responseString);
                    logger.Log(new LogMessage(Category.Information, "Token refreshed"));
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonMapper.ToObject<BadRequestResponse>(responseString);
                    logger.Log(new LogMessage(Category.Error, errorResponse.error));
                }
            }
            catch (Exception e)
            {
                logger.Log(new LogMessage(Category.Error, e.Message));
                logger.Log(new LogMessage(Category.Debug, $"{e.Message}"));
                logger.Log(new LogMessage(Category.Debug, $"{e.StackTrace}"));
            }

            return result;
        }

        private static Dictionary<string, string> GetDeviceCodeRequestContentDictionary(string appId, string scopes)
        {
            var deviceCodeRequestContent = new Dictionary<string, string>();
            deviceCodeRequestContent.Add("client_id", appId);
            deviceCodeRequestContent.Add("scope", scopes);

            return deviceCodeRequestContent;
        }

        private static Dictionary<string, string> GetAccessTokenRequestContentDictionary(string appId, string deviceCode)
        {
            var accessTokenRequestContent = new Dictionary<string, string>();
            accessTokenRequestContent.Add("grant_type", "urn:ietf:params:oauth:grant-type:device_code");
            accessTokenRequestContent.Add("client_id", appId);
            accessTokenRequestContent.Add("device_code", deviceCode);

            return accessTokenRequestContent;
        }

        private static Dictionary<string, string> GetRefreshTokenRequestContentDictionary(string appId, string refreshToken)
        {
            var accessTokenRequestContent = new Dictionary<string, string>();
            accessTokenRequestContent.Add("grant_type", "refresh_token");
            accessTokenRequestContent.Add("client_id", appId);
            accessTokenRequestContent.Add("refresh_token", refreshToken);

            return accessTokenRequestContent;
        }
    }
}
