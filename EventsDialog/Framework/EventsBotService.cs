using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventsBot.Dialogs.ThirdParty.Null;
using EventsBot.Interfaces;

namespace EventsBot.Dialogs.Framework
{
    public class EventsBotServiceAttribute : Attribute
    {   
        public EventsBotServiceAttribute(Type eventDiscoveryService ):this(eventDiscoveryService,typeof(NullRegistrationDialog))
        {
        }

        public EventsBotServiceAttribute(Type eventDiscoveryService, Type eventRegistrationDialog )
        {
            this.EventDiscoveryService = eventDiscoveryService;
            this.EventRegistrationDialog = eventRegistrationDialog;
        }

        public Type EventDiscoveryService { get; }
        public Type EventRegistrationDialog { get; }
    }
}