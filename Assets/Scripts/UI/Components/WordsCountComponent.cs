using Events.Core;
using Events.Rounds;
using Events.Score;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Components
{
    public class WordsCountComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        public UnityEvent OnEventSucceeded; 

        #region Monobehaviour
        private void OnEnable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted; 
            Bus<ScoringSequenceStartedEvent>.OnEvent += HandleOnScoringStarted; 
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted; 
            Bus<ScoringSequenceStartedEvent>.OnEvent -= HandleOnScoringStarted;
        }
        #endregion

        private void HandleOnRoundStarted(RoundStartedEvent args)
        {
            UpdateLabel();
        }
        
        private void HandleOnScoringStarted(ScoringSequenceStartedEvent evt)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            text.text = GetLabel(GameManager.Instance.Run.Round.WordsRemaining);
            OnEventSucceeded?.Invoke(); 
        }

        private string GetLabel(int count) => $"words left: {count}";
    }
}