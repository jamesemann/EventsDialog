using System.Collections.Generic;
using EventsDialog.Interfaces;
using Microsoft.Bot.Connector;

namespace EventsBot.Extensions
{
    public static class CardExtensions
    {
        public static Attachment CreateHeroCard(this EventListing eventListing)
        {
            var cardImages1 = new List<CardImage> {new CardImage(eventListing.Image)};
            var cardButtons = new List<CardAction>();

            if (eventListing.RegistrationSupported)
            {
                var plButton = new CardAction(ActionTypes.PostBack, $"Register",
                    value: $"cmd://register/{eventListing.Id}");
                cardButtons.Add(plButton);
            }

            var plCard1 = new HeroCard {Title = eventListing.Title, Subtitle = eventListing.Summary, Images = cardImages1, Buttons = cardButtons};
            return plCard1.ToAttachment();
        }
    }
}