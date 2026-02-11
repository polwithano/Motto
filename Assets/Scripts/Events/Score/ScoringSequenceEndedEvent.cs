using Events.Core;
using Models;

namespace Events.Score
{
    public class ScoringSequenceEndedEvent : IEvent
    {
        public ScoreLog ScoreLog { get; private set; }
        
        public ScoringSequenceEndedEvent(ScoreLog scoreLog)
        {
            ScoreLog = scoreLog;
        }
    }
}