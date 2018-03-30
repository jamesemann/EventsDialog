using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventsBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EventsBot.Dialogs
{
    [Serializable]
    public class DiscoverDialog : IDialog<object>
    {
        public int Month;
        public int Year;

        public DiscoverDialog(string month)
        {
            if (month != null) (Year, Month) = month.GetYearAndMonth();
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (Month == 0)
            {
                await context.SayAsync($"Which month are you interested in?");

                context.Wait(MonthDisambiguatedCallback);
            }
            else
            {
                await ShowEventsForMonth(context);
            }
        }

        public async Task MonthDisambiguatedCallback(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            (Year, Month) = ((Activity) await argument).Text.GetYearAndMonth();

            if (Month == 0)
            {
                await context.Activity.SendTypingActivity();
                await context.SayAsync($"Which month are you interested in?");

                context.Wait(MonthDisambiguatedCallback);
            }
            else
            {
                await ShowEventsForMonth(context);
            }
        }

        private async Task ShowEventsForMonth(IDialogContext context)
        {
            var carouselMessage = context.MakeMessage();
            carouselMessage.AttachmentLayout = "carousel";

            await context.Activity.DoWithTyping(async () =>
            {
                var fromDate = new DateTime(Year,Month,1,0,0,0);
                var toDate = new DateTime(Year,Month,DateTime.DaysInMonth(Year,Month),23,59,59);
                var eventsInMonth = (await ServiceLocator.EventDiscovery.GetEventListingsAsync(fromDate, toDate)).ToList();

                foreach (var eventInMonth in eventsInMonth)
                {
                    carouselMessage.Attachments.Add(eventInMonth.CreateHeroCard());
                }

                if (eventsInMonth.Any())
                {
                    await context.SayAsync($"The following events have been found for {new DateTime(Year, Month, 1):MMMM yyyy}:");
                    await context.PostAsync(carouselMessage);
                }
                else 
                {
                    await context.SayAsync($"No events have been found for {new DateTime(Year, Month, 1):MMMM yyyy}");
                }
            });

            context.Done(new object());
        }
    }
}