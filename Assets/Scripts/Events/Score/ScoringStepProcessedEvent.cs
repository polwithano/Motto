using System;
using Events.Core;
using Models;

namespace Events.Score
{
    public struct ScoringStepProcessedEvent : IEvent
    {
        public ScoreLogEntry Entry { get; private set; }
        public Action Callback {get; private set;}

        public ScoringStepProcessedEvent(ScoreLogEntry entry, Action callback)
        {
            Entry = entry;
            Callback = callback;
        }
    }
}