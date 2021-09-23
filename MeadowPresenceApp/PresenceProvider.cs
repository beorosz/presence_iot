using LitJson;
using MeadowPresenceApp.Communication;
using MeadowPresenceApp.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MeadowPresenceApp
{
    public interface IPresenceProvider
    {
        Task<PresenceResponse> GetPresence();
    }

    public class PresenceProvider : HttpCommunicationBase, IPresenceProvider
    {
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly ILogger logger;
        private readonly string getPresenceUri;

        public PresenceProvider(IAccessTokenProvider accessTokenProvider, ILogger logger)
        {
            this.accessTokenProvider = accessTokenProvider;
            this.logger = logger;
            getPresenceUri = "https://graph.microsoft.com/v1.0/me/presence";
        }

        public async Task<PresenceResponse> GetPresence()
        {
            PresenceResponse result = new PresenceResponse();
            var retryPresenceRequest = true;

            while (retryPresenceRequest)
            {
                try
                {
                    var accessToken = await accessTokenProvider.GetAccessToken();

                    var httpResponse = await HttpSendWithBearerAccessToken(HttpMethod.Get, getPresenceUri, accessToken);
                    var responseString = await httpResponse.Content.ReadAsStringAsync();
                    
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        result = JsonMapper.ToObject<PresenceResponse>(responseString);
                        retryPresenceRequest = false;
                    }
                    else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var errorResponse = UnauthorizedResponseJsonReader.ToObject(responseString);
                        if (errorResponse.Error.Code == "InvalidAuthenticationToken")
                        {
                            retryPresenceRequest = true;
                            logger.Log(Category.Information, "Refreshing token");
                            await accessTokenProvider.RefreshAccessToken();
                        }
                        else
                        {
                            retryPresenceRequest = false;
                            logger.Log(Category.Error, "Refresh error");
                        }
                    }
                    else
                    {
                        retryPresenceRequest = false;
                        logger.Log(Category.Error, $"Unexpected error: {httpResponse.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    logger.Log(Category.Error, e.Message);                    
                }
            }

            return result;
        }
    }
}
