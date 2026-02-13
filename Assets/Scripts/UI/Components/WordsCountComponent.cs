using Events.Rounds;
using Events.Score;
using Managers;
using UI.Components.Core;

namespace UI.Components
{
    public class WordsCountComponent 
        : MultiEventReactiveLabelComponent<int>
    {
        protected override void RegisterEvents()
        {
            ListenTo<RoundStartedEvent>();
            ListenTo<ScoringSequenceStartedEvent>();
        }

        protected override int GetValue()
        {
            return GameManager.Instance.Run.Round.WordsRemaining;
        }
    }
}