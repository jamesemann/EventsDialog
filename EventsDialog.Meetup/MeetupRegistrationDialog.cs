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

namespace EventsDialog.Meetup
{
    [Serializable]
    public class MeetupRegistrationDialog : EventRegistrationDialog
    {
        public override async Task RegisterForEventListingAsync(IDialogContext context, string eventListingId)
        {
            var redirectUrl = ConfigurationManager.AppSettings["meetupRedirectUrl"];
            var clientId = ConfigurationManager.AppSettings["meetupCliendId"];
            var groupUrlName = ConfigurationManager.AppSettings["meetupGroupUrl"];

            if (string.IsNullOrEmpty(redirectUrl))
            {
                throw new Exception(@"populate appsetting <add key=""meetupRedirectUrl"" value=""""/>");
            }
            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception(@"populate appsetting <add key=""meetupCliendId"" value=""""/>");
            }
            if (string.IsNullOrEmpty(groupUrlName))
            {
                throw new Exception(@"populate appsetting <add key=""meetupGroupUrl"" value=""""/>");
            }

            var userId = context.Activity.Recipient.Id;
            var existing = TableStorage.RetrieveByUserId(userId);
            
            if (existing == null)
            {
                await context.SayAsync($"https://secure.meetup.com/oauth2/authorize?scope=rsvp&client_id={clientId}&response_type=code&redirect_uri={redirectUrl}?userId={userId}");
                
                // TODO wait for auth flow to complete
                while (existing == null)
                {
                    Thread.Sleep(5000);
                    existing = TableStorage.RetrieveByUserId(userId);
                }
                // end TODO
                
                await Rsvp(context, existing.AccessToken, groupUrlName, eventListingId);
                context.Done<object>(new object());
            }

            // subtract 1 hour to ensure is an overlap
            else if (existing.ExpiryDateTime <= DateTime.Now.AddMinutes(-60))
            {
                var refreshToken = await AuthController.GetRefreshToken(userId, existing.RefreshToken);
                TableStorage.InsertOrUpdate(refreshToken);

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

                var uriBuilder = new UriBuilder("https", "api.meetup.com", 443, $"/{urlName}/events/{eventId}/rsvps","?response=yes");
                
                var response = await httpClient.PostAsync(uriBuilder.Uri, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));

                if (response.IsSuccessStatusCode)
                {
                    await context.SayAsync("You have been registered");
                }
                else
                {
                    throw new Exception($"authentication error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}