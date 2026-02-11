using Models;

namespace Events.Score
{
    public struct ScoringStepStartedEvent
    {
        public ScoreLogEntry Entry { get; private set; }
        
        public ScoringStepStartedEvent(ScoreLogEntry entry)
        {
            Entry = entry;
        }
    }
}