using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
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
                        (var registrationSupported, var registrationSupportedReason) = IsRegistrationSupported(evt);

                        var listing = new EventListing();

                        //var rsvpLimit = evt.SelectToken("$.rsvp_limit")?.Value<int>();
                        //var yesRsvpCount = evt.SelectToken("$.yes_rsvp_count")?.Value<int>();

                        //var rspvs = rsvpLimit != null ? $"{yesRsvpCount}/{rsvpLimit}" : $"{yesRsvpCount}";
                        //listing.Subtitle = $"({rspvs})";
                        listing.Summary = $"{evt.SelectToken("$.description")?.Value<string>()?.StripHtml()}";
                        listing.Id = evt.SelectToken("$.id")?.Value<string>();
                        listing.Title = evt.SelectToken("$.name")?.Value<string>();
                        listing.Image = evt.SelectToken("$.group.key_photo.photo_link")?.Value<string>();
                        listing.RegistrationSupported = registrationSupported;
                        result.Add(listing);
                    }
                }
                
                return result;
            }
        }

        private static (bool isSupported, string reason) IsRegistrationSupported(JToken evt)
        {

            var date = DateTime.ParseExact(evt.SelectToken("$.local_date")?.Value<string>(), "yyyy-MM-dd", new DateTimeFormatInfo());

            // first check rsvp_limit
            var rsvpLimit = evt.SelectToken("$.rsvp_limit")?.Value<int>();
            if (rsvpLimit != null)
            {
                var yesRsvpCount = evt.SelectToken("$.yes_rsvp_count")?.Value<int>();
                if (yesRsvpCount >= rsvpLimit)
                {
                    return (false, $"This event is full");
                }
            }

            // now check registration window
            var openOffset = evt.SelectToken("$.rsvp_open_offset")?.Value<string>();
            var closeOffset = evt.SelectToken("$.rsvp_open_offset")?.Value<string>();
            if (openOffset != null || closeOffset != null)
            {
                var rsvpOpenOffset = openOffset != null ? XmlConvert.ToTimeSpan(openOffset) : TimeSpan.FromTicks(0);
                var rsvpCloseOffset = closeOffset != null ? XmlConvert.ToTimeSpan(closeOffset) : TimeSpan.FromTicks(0);

                // if openOffset/closeOffset is applicable, then you can only register during that period
                var now = DateTime.Now;
                if (now < date + rsvpOpenOffset)
                {
                    return (false, $"Registration opens on {(date + rsvpOpenOffset).ToString("dd MMMM yyyy")}");
                }
                if (now > date - rsvpCloseOffset)
                {
                    return (false, $"Registration closed on {(date - rsvpCloseOffset).ToString("dd MMMM yyyy")}");
                }
            }
            return (true, string.Empty);
        }
    }
}