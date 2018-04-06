using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventsDialog.Meetup
{
    public class OAuthUtility
    {
        private static string _redirectUrl;
        private static string _clientId;
        private static string _groupUrlName;
        private static string _clientSecret;

        static OAuthUtility()
        {
            _redirectUrl = ConfigurationManager.AppSettings["meetupRedirectUrl"];
            _clientId = ConfigurationManager.AppSettings["meetupClientId"];
            _groupUrlName = ConfigurationManager.AppSettings["meetupGroupUrl"];
            _clientSecret = ConfigurationManager.AppSettings["meetupClientSecret"];

            if (string.IsNullOrEmpty(_redirectUrl))
            {
                throw new Exception(@"populate appsetting <add key=""meetupRedirectUrl"" value=""""/>");
            }
            if (string.IsNullOrEmpty(_clientId))
            {
                throw new Exception(@"populate appsetting <add key=""meetupClientId"" value=""""/>");
            }
            if (string.IsNullOrEmpty(_groupUrlName))
            {
                throw new Exception(@"populate appsetting <add key=""meetupGroupUrl"" value=""""/>");
            }
            if (string.IsNullOrEmpty(_clientSecret))
            {
                throw new Exception(@"populate appsetting <add key=""meetupClientSecret"" value=""""/>");
            }
        }

        public static async Task<AccessResponse> GetRefreshToken(string userId, string refreshToken)
        {
            using (var httpClient = new HttpClient())
            {
                var uriBuilder = new UriBuilder("https", "secure.meetup.com", 443, $"/oauth2/access");

                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken)
                };

                var response = await httpClient.PostAsync(uriBuilder.Uri, new FormUrlEncodedContent(nvc));
                if (response.IsSuccessStatusCode)
                {
                    var accessResponse = JsonConvert.DeserializeObject<AccessResponse>(await response.Content.ReadAsStringAsync());
                    accessResponse.UserId = userId;
                    accessResponse.ExpiryDateTime = DateTime.Now.AddMinutes(accessResponse.ExpiresIn);
                    return accessResponse;
                }
                else
                {
                    throw new Exception($"authentication error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }

        public static async Task<AccessResponse> GetAccessToken(string userId, string code)
        {
            var redirectUrl = ConfigurationManager.AppSettings["meetupRedirectUrl"];

            using (var httpClient = new HttpClient())
            {
                var uriBuilder = new UriBuilder("https", "secure.meetup.com", 443, $"/oauth2/access");

                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", $"{redirectUrl}?userId={userId}"),
                    new KeyValuePair<string, string>("code", code)
                };

                var response = await httpClient.PostAsync(uriBuilder.Uri, new FormUrlEncodedContent(nvc));
                if (response.IsSuccessStatusCode)
                {
                    var accessResponse = JsonConvert.DeserializeObject<AccessResponse>(await response.Content.ReadAsStringAsync());
                    accessResponse.UserId = userId;
                    accessResponse.ExpiryDateTime = DateTime.Now.AddMinutes(accessResponse.ExpiresIn);
                    return accessResponse;
                }
                else
                {
                    throw new Exception($"authentication error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}
