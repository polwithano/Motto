using System;
using Events.Core;
using Models;

namespace Events.Score
{
    public struct ScoreStepProcessedEvent : IEvent
    {
        public ScoreLogEntry Entry { get; private set; }
        public Action Callback {get; private set;}

        public ScoreStepProcessedEvent(ScoreLogEntry entry, Action callback)
        {
            Entry = entry;
            Callback = callback;
        }
    }
}