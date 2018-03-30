using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EventsBot.Interfaces
{
    public abstract class EventDiscoveryService
    {
        public abstract Task<IEnumerable<EventListing>> GetEventListingsAsync(DateTime from, DateTime to);        
    }
}