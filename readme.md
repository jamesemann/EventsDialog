# EventsDialog
Events template for Microsoft Bot Framework chatbots.  Currently supports dicovering events (e.g. from api.meetup.com) and registering for those events.
https://www.nuget.org/packages/EventsDialog/

## Usage

Before getting started. Deploy the luis.ai model from https://github.com/jamesemann/EventsDialog/blob/master/events.json to your luis.ai subscription.  Follow the instructions here-->https://docs.microsoft.com/en-us/azure/cognitive-services/luis/create-new-app#import-new-app if you need any help with this step.

Once you have your model, create a new bot framework app. You need to add the following appSettings to your bot framework app. You can get both of these from luis.ai after you have imported the model:

```
<add key="luisId" value="<YOUR LUIS ID>" />
<add key="luisKey" value="<YOUR LUIS KEY>" />
```

Now you can install the NuGet package:

```
Install-Package EventsDialog
```

EventsDialog can provide event discovery and event registration by use of two components provided by you.

### Event discovery

To support event discovery, implement `EventDiscoveryService`. This interface requires one method which returns a set of `EventListing` objects. You can do whatever API/DB lookup you need here.  

Here's a sample fake service:

```
using EventsBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsBot.Dialogs
{
    public class FakeEventDiscoveryService : EventDiscoveryService
    {
        public override async Task<IEnumerable<EventListing>> GetEventListingsAsync(DateTime fromDate, DateTime toDate)
        {
            return new EventListing[]
            {
                new EventListing() { Id = "1", Title = "Event 1", Image = "https://avatars1.githubusercontent.com/u/6830648?s=460&v=4", Summary = "an event"},
                new EventListing() { Id = "2", Title = "Event 2", Image = "https://avatars1.githubusercontent.com/u/6830648?s=460&v=4", Summary = "an event"},
                new EventListing() { Id = "3", Title = "Event 3", Image = "https://avatars1.githubusercontent.com/u/6830648?s=460&v=4", Summary = "an event"}
            };
        }
    }
}
```

### Event registration

To support event registration, subclass `EventRegistrationDialog`. This abstract class provides the hooks required to initiate a dialog after the user has expressed their intent to register.  Because this is a standard bot framework dialog you can make the process as simple/complex as you need. Here's a sample:

```
using System;
using System.Threading.Tasks;
using EventsBot.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EventsBot.Dialogs.ThirdParty.Meetup
{
    [Serializable]
    public class FakeRegistrationDialog : EventRegistrationDialog
    {
        public override async Task RegisterForEventListingAsync(IDialogContext context, string eventListingId)
        {
            await context.SayAsync($"Let me register you for {EventId}.  What's your name?");
            context.Wait(NameReceivedAsync);
        }

        private async Task NameReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            await context.SayAsync($"Great, you are registered {activity.Text}");
            context.Done<object>("done");
        }
    }
}
```

### Wiring it up

Once you have created your event discovery and (optionally) event registration. You now just need to wire everything up.  To do that, just subclass `EventsDialog` and add the `EventsBotService` attribute.  Point the attribute to your event discovery/registration classes.  Here's a sample:

```
using EventsBot.Dialogs.Framework;
using EventsBot.Dialogs.ThirdParty.Meetup;
using System;

namespace EventsBot.Dialogs
{
    [Serializable]
    [EventsBotService(typeof(FakeEventDiscoveryService), typeof(FakeRegistrationDialog))]
    public class RootDialog : EventsDialog
    {
    }
}
```