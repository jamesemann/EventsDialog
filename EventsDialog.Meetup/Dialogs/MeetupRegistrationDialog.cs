using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EventsDialog.Interfaces;
using EventsDialog.Meetup.Controllers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventsDialog.Meetup
{
    [Serializable]
    public class MeetupRegistrationDialog : EventRegistrationDialog
    {
        public override async Task RegisterForEventListingAsync(IDialogContext context, string eventListingId)
        {
            var redirectUrl = ConfigurationManager.AppSettings["meetupRedirectUrl"];
            var clientId = ConfigurationManager.AppSettings["meetupClientId"];
            var groupUrlName = ConfigurationManager.AppSettings["meetupGroupUrl"];

            if (string.IsNullOrEmpty(redirectUrl))
            {
                throw new Exception(@"populate appsetting <add key=""meetupRedirectUrl"" value=""""/>");
            }
            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception(@"populate appsetting <add key=""meetupClientId"" value=""""/>");
            }
            if (string.IsNullOrEmpty(groupUrlName))
            {
                throw new Exception(@"populate appsetting <add key=""meetupGroupUrl"" value=""""/>");
            }

            var userId = context.Activity.Recipient.Id;
            var existing = TableStorageService.RetrieveByUserId(userId);

            if (existing == null)
            {
                var authMessage = context.MakeMessage();
                authMessage.Attachments.Add(await CardsUtility.CreateAuthCard(clientId, redirectUrl, userId));
                await context.PostAsync(authMessage);

                // TODO wait for auth flow to complete
                var start = DateTime.Now;
                while (existing == null && (DateTime.Now - start).TotalSeconds < 120)
                {
                    Thread.Sleep(5000);
                    existing = TableStorageService.RetrieveByUserId(userId);
                }
                // end TODO

                if (existing != null)
                {
                    await Rsvp(context, existing.AccessToken, groupUrlName, eventListingId);
                }
                else
                {
                    await context.SayAsync("Timed out waiting for Meetup.com authentication to complete. Please try again.");
                }
                context.Done<object>(new object());
            }

            // subtract 1 hour to ensure is an overlap
            else if (existing.ExpiryDateTime <= DateTime.Now.AddMinutes(-60))
            {
                var refreshToken = await OAuthUtility.GetRefreshToken(userId, existing.RefreshToken);
                TableStorageService.InsertOrUpdate(refreshToken);

                await Rsvp(context, existing.AccessToken, groupUrlName, eventListingId);
                context.Done<object>(new object());
            }
            else
            {
                await Rsvp(context, existing.AccessToken, groupUrlName, eventListingId);
                context.Done<object>(new object());
            }
        }

        public async Task Rsvp(IDialogContext context, string bearerToken, string urlName, string eventId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                var uriBuilder = new UriBuilder("https", "api.meetup.com", 443, $"/{urlName}/events/{eventId}/rsvps", "?response=yes");

                var response = await httpClient.PostAsync(uriBuilder.Uri, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));

                if (response.IsSuccessStatusCode)
                {
                    var evt = JObject.Parse(await response.Content.ReadAsStringAsync());
                    var eventName = evt.SelectToken("$.event.name")?.Value<string>();
                    var venueName = evt.SelectToken("$.venue.name")?.Value<string>();
                    var venueAddress1 = evt.SelectToken("$.venue.address_1")?.Value<string>();
                    var venueCity = evt.SelectToken("$.venue.city")?.Value<string>();
                    var venueZip = evt.SelectToken("$.venue.zip")?.Value<string>();
                    var venueCountry = evt.SelectToken("$.venue.country")?.Value<string>();
                    var lat = evt.SelectToken("$.venue.lat")?.Value<decimal>();
                    var lon = evt.SelectToken("$.venue.lon")?.Value<decimal>();

                    var authMessage = context.MakeMessage();
                    authMessage.Attachments.Add(await CardsUtility.CreateMapCard(eventName, venueName, venueAddress1, venueCity, venueZip, venueCountry, lat, lon));
                    await context.PostAsync(authMessage);
                }
                else
                {
                    await context.SayAsync($"Authentication error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}