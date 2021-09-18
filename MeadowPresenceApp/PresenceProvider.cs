using LitJson;
using MeadowPresenceApp.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MeadowPresenceApp
{
    public interface IPresenceProvider
    {
        Task<PresenceResponse> GetPresence();
    }

    public class PresenceProvider : IPresenceProvider
    {
        private readonly IAccessTokenProvider accessTokenProvider;

        public PresenceProvider(IAccessTokenProvider accessTokenProvider)
        {
            this.accessTokenProvider = accessTokenProvider;
        }

        public async Task<PresenceResponse> GetPresence()
        {
            PresenceResponse result = new PresenceResponse();

            var accessToken = await accessTokenProvider.GetAccessToken();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 30);
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/presence");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var httpResponse = await client.SendAsync(request);
                    httpResponse.EnsureSuccessStatusCode();

                    var responseString = await httpResponse.Content.ReadAsStringAsync();
                    result = JsonMapper.ToObject<PresenceResponse>(responseString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return result;
        }
    }
}
