# EventsDialog

EventsDialog provides a simple template to quickly allow developers to connect users to events (e.g. on Meetup.com).  It will recognise the users intent, extracting dates and disambiguating where necessary.

It's implemented as a template for Microsoft Bot Framework chatbots.  
Currently supports discovering events and registering for those events.
https://www.nuget.org/packages/EventsDialog/


## Getting started

First, deploy the luis.ai model from https://github.com/jamesemann/EventsDialog/blob/master/events.json to your luis.ai subscription.  Follow the instructions here-->https://docs.microsoft.com/en-us/azure/cognitive-services/luis/create-new-app#import-new-app if you need any help with this step.

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


## Event discovery/registration using pre-built connectors

To use pre-built discovery/registration connectors, you follow a simple process.  This example shows you how to add the Meetup connector.

### Install the connector

```
Install-Package EventsDialog.Meetup
```

### Add your EventsDialog

To query Meetup, create a dialog that subclasses `EventsDialog` and add the `EventsBotService` attribute.  Point the attribute to the Meetup event discovery class and meetup registration dialog:

```
using EventsDialog.Dialogs.Framework;
using EventsDialog.Dialogs.Meetup;
using System;

namespace EventsBot.Dialogs
{
    [Serializable]
    [EventsBotService(typeof(MeetupEventDiscoveryService), typeof(MeetupRegistrationDialog))]
    public class RootDialog : EventsDialog
    {
    }
}
```

### Add any connector-specific configuration

The Meetup connector requires an API key (get from https://www.meetup.com/meetup_api/), a Group Name URL (the canonical name of the Meetup group), OAuth client ID and secret (get them from https://secure.meetup.com/meetup_api/oauth_consumers/), an Azure table storage account to store users auth/refresh tokens, and a google maps API key if you want to show the location on a map.  Add them to your appSettings:

```
<add key="meetupKey" value="<YOUR MEETUP KEY>" />
<add key="meetupGroupUrl" value="<YOUR MEETUP GROUP URL>" />
<add key="meetupClientId" value="<YOUR MEETUP OAUTH CLIENT ID>" />
<add key="meetupClientSecret" value="<YOUR MEETUP OAUTH CLIENT SECRET>" />
<add key="meetupRedirectUrl" value="<YOUR PUBLIC BOT APPLICATION URL>/api/auth"/>
<add key="meetupTableStorageConnectionString" value="<YOUR AZURE STORAGE CONNECTION STRING>" />
<add key="meetupGoogleMapsApiKey" value="<YOUR GOOGLE MAPS API KEY>" />
```

You are now ready to test it!

### Event discovery/registration using your own connectors

If you want to connect to your own events system, then you can do this by creating your own connector providing event discovery and optionally event registration.

To support event discovery, implement `EventDiscoveryService`. This interface requires one method which returns a set of `EventListing` objects. You can do whatever API/DB lookup you need here.  

Here's a sample fake service:

```
using EventsDialog.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsDialog.Dialogs.Fake
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
using EventsDialog.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EventsDialog.Dialogs.Fake
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
using EventsDialog.Dialogs.Framework;
using EventsDialog.Dialogs.Fake;
using System;

namespace EventsDialog.Dialogs
{
    [Serializable]
    [EventsBotService(typeof(FakeEventDiscoveryService), typeof(FakeRegistrationDialog))]
    public class RootDialog : EventsDialog
    {
    }
}
```
