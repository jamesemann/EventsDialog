using System;

namespace EventsDialog.Framework
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