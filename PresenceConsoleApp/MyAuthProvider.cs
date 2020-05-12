using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PresenceConsoleApp
{
    public class MyAuthProvider : IAuthenticationProvider
    {
        public IConfidentialClientApplication clientApp;
        private string[] _scopes;
        private IAccount _userAccount;

        public MyAuthProvider(string appId, string tenantId, string clientSecret)
        {
            //AuthenticationResult authResult = null;

            clientApp = ConfidentialClientApplicationBuilder.Create(appId)
               .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
               .WithClientSecret(clientSecret)
               .Build();                       
        }
            

            
            //try
            //{
            //    authResult = await clientApp.AcquireTokenForClient(scopes)
            //        .ExecuteAsync();
            //    Console.WriteLine(authResult);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}
        public async Task<string> GetAccessToken()
        {
            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Invoke device code flow so user can sign-in with a browser
                    var result = await clientApp.AcquireTokenForClient(_scopes).ExecuteAsync();

                    _userAccount = result.Account;
                    return result.AccessToken;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error getting access token: {exception.Message}");
                    return null;
                }
            }
            else
            {
                // If there is an account, call AcquireTokenSilent
                // By doing this, MSAL will refresh the token automatically if
                // it is expired. Otherwise it returns the cached token.

                var result = await clientApp
                    .AcquireTokenSilent(_scopes, _userAccount)
                    .ExecuteAsync();

                return result.AccessToken;
            }
        }

        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}