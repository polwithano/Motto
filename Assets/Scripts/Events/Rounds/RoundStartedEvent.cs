using Events.Core;
using Models;
using Models.Rounds;

namespace Events.Rounds
{
    public struct RoundStartedEvent : IEvent
    {
        public RoundContext Context { get; private set; }
        
        public RoundStartedEvent(RoundContext context) => Context = context;
    }
}