using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsDialog.Interfaces
{
    public abstract class EventDiscoveryService
    {
        public abstract Task<IEnumerable<EventListing>> GetEventListingsAsync(DateTime from, DateTime to);        
    }
}