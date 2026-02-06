using Events.Core;
using Models;

namespace Events.Rounds
{
    public enum RoundEndedStatus
    {
        Success, 
        Failure
    }
    
    public struct RoundEndedEvent : IEvent
    {
        public RoundEndedStatus Status { get; private set; }
        public RoundContext Context    { get; private set; }

        public RoundEndedEvent(RoundEndedStatus status, RoundContext context)
        {
            Status = status;
            Context = context;
        }
    }
}