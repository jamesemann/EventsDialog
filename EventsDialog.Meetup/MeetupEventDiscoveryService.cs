using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventsDialog.Extensions;
using EventsDialog.Interfaces;
using Newtonsoft.Json.Linq;

namespace EventsDialog.Meetup
{
    // <add key="meetupKey" value=""/>
    // <add key="meetupGroupUrl" value=""/>
    public class MeetupEventDiscoveryService : EventDiscoveryService
    {
        private readonly string _groupUrlName;

        private readonly string _key;

        public MeetupEventDiscoveryService()
        {
            _key = ConfigurationManager.AppSettings["meetupKey"];
            _groupUrlName = ConfigurationManager.AppSettings["meetupGroupUrl"];

            if (string.IsNullOrEmpty(_key))
            {
                throw new Exception(@"populate appsetting <add key=""meetupKey"" value=""""/>");
            }
            if (string.IsNullOrEmpty(_groupUrlName))
            {
                throw new Exception(@"populate appsetting <add key=""meetupGroupUrl"" value=""""/>");
            }
        }

        public override async Task<IEnumerable<EventListing>> GetEventListingsAsync(DateTime fromDate, DateTime toDate)
        {
            using (var httpClient = new HttpClient())
            {
                var formattedFrom = fromDate.ToString("yyyy-MM-ddTHH:mm:ss");
                var formattedTo = toDate.ToString("yyyy-MM-ddTHH:mm:ss");

                var uriBuilder = new UriBuilder("https", "api.meetup.com", 443, $"{_groupUrlName}/events")
                {
                    Query =
                        $"fields=featured_photo&start_date_range={formattedFrom}&end_date_range={formattedTo}&page=5&sign=true&key={_key}"
                };

                var response = await httpClient.GetStringAsync(uriBuilder.Uri);

                return from evt in JArray.Parse(response)
                    select new EventListing
                    {
                        Id = evt.SelectToken("$.id")?.Value<string>(),
                        Title = evt.SelectToken("$.name")?.Value<string>(),
                        Image = evt.SelectToken("$.featured_photo.photo_link")?.Value<string>(),
                        Summary = evt.SelectToken("$.description")?.Value<string>()?.StripHtml()
                    };
            }
        }
    }
}