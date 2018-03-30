using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace EventsBot.Interfaces
{
    [Serializable]
    public abstract class EventRegistrationDialog : IDialog<object>
    {
        public string EventId { get; set; }

        public abstract Task RegisterForEventListingAsync(IDialogContext context, string eventListingId);
        
        public async Task StartAsync(IDialogContext context)
        {
            if (EventId == null)
            {
                throw new Exception("EventId has not been set, needs to be set before starting the Dialog.");
            }
            await RegisterForEventListingAsync(context, EventId);
        }
    }
}