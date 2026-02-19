using Events.Core;
using Models;

namespace Events.Score
{
    public struct ScoringSequenceEndedEvent : IEvent
    {
        public ScoreLog ScoreLog { get; private set; }
        
        public ScoringSequenceEndedEvent(ScoreLog scoreLog)
        {
            ScoreLog = scoreLog;
        }
    }
}