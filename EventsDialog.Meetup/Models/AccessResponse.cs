using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace EventsDialog.Meetup
{
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
