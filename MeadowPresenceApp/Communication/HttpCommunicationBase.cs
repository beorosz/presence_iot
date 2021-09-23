using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MeadowPresenceApp.Communication
{
    public abstract class HttpCommunicationBase
    {
        protected async Task<HttpResponseMessage> HttpSend(HttpMethod httpMethod, string uri, FormUrlEncodedContent formUrlEncodedContent)
        {
            HttpResponseMessage httpResponse;

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 30);
                var request = new HttpRequestMessage(httpMethod, uri)
                {
                    Content = formUrlEncodedContent
                };
                httpResponse = await client.SendAsync(request);
            }

            return httpResponse;
        }

        protected async Task<HttpResponseMessage> HttpSendWithBearerAccessToken(HttpMethod httpMethod, string uri, string accessToken)
        {
            HttpResponseMessage httpResponse;

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 30);
                var request = new HttpRequestMessage(httpMethod, uri);
                //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpResponse = await client.SendAsync(request);
            }

            return httpResponse;
        }
    }
}
