using System.Threading.Tasks;
using EventsBot.Interfaces;
using Microsoft.Bot.Builder.Dialogs;

namespace EventsBot.Dialogs.ThirdParty.Null
{
    public class NullRegistrationDialog : EventRegistrationDialog
    {
        public override async Task RegisterForEventListingAsync(IDialogContext context, string eventListingId)
        {
            context.Done<object>(new object());
        }
    }
}