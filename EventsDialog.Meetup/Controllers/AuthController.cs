using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EventsDialog.Extensions;
using EventsDialog.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventsDialog.Meetup.Controllers
{
    public class AuthController: ApiController
    {
        private static string _redirectUrl;
        private static string _clientId;
        private static string _groupUrlName;
        private static string _clientSecret;

        static AuthController()
        {
            _redirectUrl = ConfigurationManager.AppSettings["meetupRedirectUrl"];
            _clientId = ConfigurationManager.AppSettings["meetupCliendId"];
            _groupUrlName = ConfigurationManager.AppSettings["meetupGroupUrl"];
            _clientSecret = ConfigurationManager.AppSettings["meetupClientSecret"];

            if (string.IsNullOrEmpty(_redirectUrl))
            {
                throw new Exception(@"populate appsetting <add key=""meetupRedirectUrl"" value=""""/>");
            }
            if (string.IsNullOrEmpty(_clientId))
            {
                throw new Exception(@"populate appsetting <add key=""meetupCliendId"" value=""""/>");
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

        public async Task<HttpResponseMessage> Get(string userId, string code) //userId is botframework user id
        {
            var accessToken = await GetAccessToken(userId, code);
            TableStorage.InsertOrUpdate(accessToken);

            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("Thank you, you may now close this window.")
            };
            return response;
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

    public class AccessResponse : TableEntity
    {
        public AccessResponse()
        {
            this.PartitionKey = "all";
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                RowKey = value;
            }
        }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        public DateTime ExpiryDateTime { get; set; }
    }
}
