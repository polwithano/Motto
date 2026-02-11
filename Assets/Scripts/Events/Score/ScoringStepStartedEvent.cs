using Events.Core;
using Models;

namespace Events.Score
{
    public struct ScoringStepStartedEvent : IEvent
    {
        public ScoreLogEntry Entry { get; private set; }
        
        public ScoringStepStartedEvent(ScoreLogEntry entry)
        {
            Entry = entry;
        }
    }
}