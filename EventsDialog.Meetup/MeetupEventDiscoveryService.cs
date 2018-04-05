using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventsDialog.Extensions;
using EventsDialog.Interfaces;
using Newtonsoft.Json.Linq;

namespace EventsDialog.Meetup
{
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
                var uriBuilder = new UriBuilder("https", "api.meetup.com", 443, $"{_groupUrlName}/events",
                    $"?fields=group_key_photo&&key={_key}");

                var response = await httpClient.GetStringAsync(uriBuilder.Uri);
                
                var result = new List<EventListing>();
                foreach (var evt in JArray.Parse(response))
                {
                    var date = DateTime.ParseExact(evt.SelectToken("$.local_date")?.Value<string>(), "yyyy-MM-dd", new DateTimeFormatInfo());
                    if (date >= fromDate && date <= toDate)
                    {

                        result.Add(new EventListing
                        {
                            Id = evt.SelectToken("$.id")?.Value<string>(),
                            Title = evt.SelectToken("$.name")?.Value<string>(),
                            Image = evt.SelectToken("$.group.key_photo.photo_link")?.Value<string>(),
                            Summary = evt.SelectToken("$.description")?.Value<string>()?.StripHtml(),
                            RegistrationSupported = true
                        });
                    }
                }

                // TODO what if there are no results, suggest alternatives?
                return result;
            }
        }
    }
}