using System;
using System.Threading.Tasks;
using EventsDialog.Framework;
using EventsDialog.Meetup;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EventsDialog.SampleBot.Dialogs
{
    [Serializable]
    [EventsBotService(typeof(MeetupEventDiscoveryService), typeof(MeetupRegistrationDialog))]
    public class RootDialog : EventsBot.Dialogs.Framework.EventsDialog
    {
    }
}