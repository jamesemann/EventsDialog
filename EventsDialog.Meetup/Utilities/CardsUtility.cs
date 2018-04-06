using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsDialog.Meetup
{
    public class CardsUtility
    {

        public static async Task<Attachment> CreateMapCard(string eventName, string venueName, string venueAddress1, string venueCity, string venueZip, string venueCountry, decimal? lat, decimal? lon)
        {
            var googleMapsApiKey = ConfigurationManager.AppSettings["meetupGoogleMapsApiKey"];
            var googleMapsLink = $"https://maps.googleapis.com/maps/api/staticmap?center={lat},{lon}&zoom=13&size=300x300&maptype=roadmap&markers=color:blue%7Clabel:S%7C{lat},{lon}&key={googleMapsApiKey}";

            var cardImages1 = new List<CardImage> { new CardImage(googleMapsLink) };

            var plCard1 = new HeroCard { Title = "Thank you for registering", Subtitle = eventName, Text = $"{venueName}{Environment.NewLine}{Environment.NewLine}{venueAddress1}{Environment.NewLine}{Environment.NewLine}{venueCity}{Environment.NewLine}{venueZip}{Environment.NewLine}{venueCountry}", Images = cardImages1 };
            return plCard1.ToAttachment();
        }

        public static async Task<Attachment> CreateAuthCard(string clientId, string redirectUrl, string userId)
        {
            var cardButtons = new List<CardAction>();

            var plButton = new CardAction(ActionTypes.OpenUrl, $"Authenticate with Meetup.com", value: $"https://secure.meetup.com/oauth2/authorize?scope=rsvp&client_id={clientId}&response_type=code&redirect_uri={redirectUrl}?userId={userId}");
            cardButtons.Add(plButton);

            var plCard1 = new HeroCard { Text = "As this is the first time you have asked to register for an event, you need to log in to Meetup.com.  Please use the button below to Authorise this chatbot to RSVP on your behalf.", Buttons = cardButtons };
            return plCard1.ToAttachment();
        }
    }
}
