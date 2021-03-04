using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PresenceConsoleApp
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        public static async Task<Presence> GetMyPresenceAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Presence
                    .Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user presence: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> GetPresence(string accessToken)
        {
            try
            {
                // GET /me/presence
                var graphApiVersion = "v9.0";
                var endpoint = $"https://graph.microsoft.com/{graphApiVersion}";
                var action = "/me/presence";
                using var client = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint + action);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting presence: {ex.Message}");
                return String.Empty;
            }

            return String.Empty;
        }
    }
}
