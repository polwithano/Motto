using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Events;
using Models;
using UI.Popup;
using UnityEngine;

namespace Managers
{
    public class ScoreManager : MonoBehaviourSingleton<ScoreManager>
    {
        [field: SerializeField] public ScoreLog Log { get; private set; }
        [SerializeField] private float logEntrySequenceDelay = 0.05f;
        
        [Header("Tile animation")]
        [SerializeField] private float raiseDistance = 0.25f;
        [SerializeField] private float raiseDuration = 0.15f;
        [SerializeField] private float settleDuration = 0.12f;
        [SerializeField] private Ease settleEase = Ease.Linear;
        [SerializeField] private Ease raiseEase = Ease.Linear;

        [Header("Popup")]
        [SerializeField] private RectTransform canvasPopup;
        [SerializeField] private ScorePopup scorePopupPrefab;
        
        #region Mono
        private void OnEnable()
        {
            GameEvents.OnScoringStarted += HandleOnScoringStarted; 
            GameEvents.OnScoreStepApplied += HandleOnScoreStepApplied;
        }

        private void OnDisable()
        {
            GameEvents.OnScoringStarted -= HandleOnScoringStarted;
            GameEvents.OnScoreStepApplied -= HandleOnScoreStepApplied;
        }
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnScoringStarted(string word, List<Tile> tiles)
        {
            Debug.Log($"HandleOnScoringStarted: {word}");
            
            Log = GenerateScoreLog(word, tiles);
            
            GameManager.Instance.Run.Round.AddScore((int)Log.Result);
            
            StartCoroutine(AnimateScore(Log));
        }

        private void HandleOnScoreStepApplied(ScoreLogEntry entry, Action onComplete)
        {
            DOVirtual.DelayedCall(logEntrySequenceDelay, () => onComplete?.Invoke());
        }
        #endregion

        private ScoreLog GenerateScoreLog(string word, List<Tile> tiles)
        {
            var log = new ScoreLog(word, tiles);
            
            // Shitty fix, first entry multiplier should be set to one
            var startValues = new List<ScoreEffect>
            {
                new ScoreEffect(ScoreEffectTarget.Score, 1),
                new ScoreEffect(ScoreEffectTarget.Modifier, 1),
            };
            var startEntry = new ScoreLogEntry(log.Logs.Count, null, startValues); 
            log.AddEntry(startEntry);
            
            var onLetterCharms = CharmManager.Instance.OnLetterCharms; 
            
            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var tileValue = new ScoreEffect(ScoreEffectTarget.Score, tile.Points); 
                var entry = new ScoreLogEntry(log.Logs.Count, tile, tileValue); 
                
                log.AddEntry(entry);

                foreach (var charm in onLetterCharms)
                {
                    if (charm.TryApplyEffect(GameManager.Instance.Run.Round, i))
                    {
                        charm.ApplyEffect(log);
                    }
                }

            }

            var tileCountEffect = new ScoreEffect(ScoreEffectTarget.Modifier, tiles.Count);
            var tileCountEntry = new ScoreLogEntry(log.Logs.Count, null, tileCountEffect);
            // Increase the multiplier based on the number of tiles
            // log.AddEntry(tileCountEntry);
            
            // --- Trigger Effects from Charms OnWordEnd
            var onWordEndCharms = CharmManager.Instance.OnWordEndCharms;
            foreach (var charm in onWordEndCharms)
            {
                if (charm.TryApplyEffect(GameManager.Instance.Run.Round))
                {
                    charm.ApplyEffect(log);
                }
            }

            log.Score = log.Logs[^1].EntryScore; 
            log.Modifier = log.Logs[^1].EntryModifier;
            log.Result = log.Score * log.Modifier; 
            
            return log; 
        }
        
        private IEnumerator AnimateScore(ScoreLog scoreLog)
        {
            foreach (var entry in scoreLog.Logs)
            {
                GameEvents.RaiseOnScoreStepStarted(entry);

                // Wait until animation completes
                bool done = false;
                GameEvents.RaiseOnScoreStepApplied(entry, () => done = true);

                // Wait for completion callback or timeout
                yield return new WaitUntil(() => done);
            }

            GameEvents.RaiseOnScoreSequenceCompleted(scoreLog);
        }
    }
}
