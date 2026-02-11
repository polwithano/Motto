using System;
using System.Collections.Generic;
using Models;

namespace Events
{
    public static class GameEvents
    {
        #region Score Events
        public static event Action<string, List<Tile>> OnScoringStarted;
        
        public static event Action<ScoreLogEntry> OnScoreStepStarted;
        public static event Action<ScoreLogEntry, Action> OnScoreStepApplied;
        public static event Action<ScoreLog> OnScoreSequenceCompleted;
        
        public static void RaiseOnScoringStarted(string word, List<Tile> tiles)
            => OnScoringStarted?.Invoke(word, tiles);
        public static void RaiseOnScoreStepStarted(ScoreLogEntry entry)
            => OnScoreStepStarted?.Invoke(entry);
        public static void RaiseOnScoreStepApplied(ScoreLogEntry entry, Action action)
            => OnScoreStepApplied?.Invoke(entry, action);
        public static void RaiseOnScoreSequenceCompleted(ScoreLog entry)
            => OnScoreSequenceCompleted?.Invoke(entry);
        #endregion
    }
}