using System.Threading.Tasks;
using EventsDialog.Interfaces;
using Microsoft.Bot.Builder.Dialogs;

namespace EventsDialog.Framework
{
    public class NullRegistrationDialog : EventRegistrationDialog
    {
        public override async Task RegisterForEventListingAsync(IDialogContext context, string eventListingId)
        {
            context.Done<object>(new object());
        }
    }
}