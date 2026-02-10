using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Game;
using Events.Rounds;
using Managers;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRoundContext : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private GameObject resultPopupPrefab; 
        [SerializeField] private Slider goalSlider;
        [SerializeField] private TextMeshProUGUI goalPercentageText;
        [SerializeField] private TextMeshProUGUI goalText; 
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI modifierText;
        [SerializeField] private TextMeshProUGUI remainingWordsText; 
        [SerializeField] private TextMeshProUGUI deckTilesCountText;
        [SerializeField] private TextMeshProUGUI redrawCountText;
        [SerializeField] private TextMeshProUGUI currencyCountText;

        [Header("Animation Settings")]
        [SerializeField] private float goalAnimationDuration = 2f;
        [SerializeField] private float punchScale = 0.25f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private int punchVibrato = 6;
        [SerializeField] private float punchElasticity = 1f;
        [SerializeField] private Ease punchEase = Ease.OutBack;
        [SerializeField] private Ease goalEase = Ease.Linear;

        private Dictionary<ScoreEffectTarget, TextMeshProUGUI> _uiByTarget;
        private RoundContext Round => GameManager.Instance.Run.Round;

        #region Mono
        private void Awake()
        {
            _uiByTarget = new Dictionary<ScoreEffectTarget, TextMeshProUGUI>
            {
                { ScoreEffectTarget.Score, scoreText },
                { ScoreEffectTarget.Modifier, modifierText }
            };
        }

        private void OnEnable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted;
            Bus<TileRedrawCompletedEvent>.OnEvent += HandleOnTileRedrawPerformed;
            Bus<CurrencyUpdatedEvent>.OnEvent += HandleOnCurrencyUpdated; 
            
            GameEvents.OnScoringStarted += HandleOnScoringStarted; 
            GameEvents.OnScoreStepStarted += HandleOnScoreStepStarted;
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent -= HandleOnRoundStarted; 
            Bus<TileRedrawCompletedEvent>.OnEvent -= HandleOnTileRedrawPerformed; 
            Bus<CurrencyUpdatedEvent>.OnEvent -= HandleOnCurrencyUpdated; 
            
            GameEvents.OnScoringStarted -= HandleOnScoringStarted; 
            GameEvents.OnScoreStepStarted -= HandleOnScoreStepStarted;
        }

        private void OnDestroy() => OnDisable();
        #endregion

        #region Event Handlers
        private void HandleOnRoundStarted(RoundStartedEvent evt)
        {
            UpdateStaticRoundData();
            AnimateRoundReset(evt.Context);
        }

        private void HandleOnScoringStarted(string word, List<Tile> tiles)
        {
            AnimateNumberText(scoreText, 0, 0.5f);
            AnimateFloatText(modifierText, 1f, 0.5f);
        }

        private void HandleOnScoreStepStarted(ScoreLogEntry entry)
        {
            UpdateScoreEffects(entry);
        }
        
        public async Task PlayScoreSequenceAsync(ScoreLog log)
        {
            UpdateRemainingWords();

            int scoreGain = (int)log.Result;
            Sequence seq = DOTween.Sequence();

            if (scoreGain > 0)
            {
                float popupDuration = AnimateScorePopup(scoreGain);
                seq.AppendInterval(popupDuration);
            }

            seq.Append(UpdateGoalProgressTween());
            await seq.AsyncWaitForCompletion();
        }

        private void HandleOnTileRedrawPerformed(TileRedrawCompletedEvent evt)
        {
            UpdateDeckAndRedrawCounts();
        }
        
        private void HandleOnCurrencyUpdated(CurrencyUpdatedEvent evt)
        {
            currencyCountText.text = $"${evt.Currency}"; 
        }
        #endregion

        #region UI Updates
        private void UpdateStaticRoundData()
        {
            deckTilesCountText.text = GameManager.Instance.Deck.DrawPile.Count.ToString();
            redrawCountText.text = Round.DrawsRemaining.ToString();
            remainingWordsText.text = Round.WordsRemaining.ToString();
            goalText.text = $"{Round.CurrentScore}/{Round.TargetScore}";
            currencyCountText.text = $"${GameManager.Instance.Run.Currency.ToString()}"; 
        }

        private void UpdateScoreEffects(ScoreLogEntry entry)
        {
            foreach (var effect in entry.ScoreEffects)
            {
                if (!_uiByTarget.TryGetValue(effect.Target, out var textElement)) continue;

                textElement.text = effect.Target switch
                {
                    ScoreEffectTarget.Score => entry.EntryScore.ToString(),
                    ScoreEffectTarget.Modifier => entry.EntryModifier.ToString("0.0"),
                    _ => textElement.text
                };

                AnimatePunch(textElement.rectTransform);
            }
        }

        public Tween UpdateGoalProgressTween()
        {
            var startValue = goalSlider.value;
            var endValue = Round.CompletionPercentage;
            var targetGoal = Round.TargetScore;

            // Create a tween sequence for the goal progress
            Sequence seq = DOTween.Sequence();

            // Animate slider
            seq.Join(goalSlider.DOValue(endValue, goalAnimationDuration).SetEase(goalEase));

            // Animate percentage text and score
            float animatedValue = startValue;
            seq.Join(DOTween.To(() => animatedValue, x =>
            {
                animatedValue = x;
                goalPercentageText.text = $"{animatedValue * 100f:0.0}%";
                goalText.text = $"{Mathf.RoundToInt(animatedValue * targetGoal)}/{targetGoal}";
            }, endValue, goalAnimationDuration).SetEase(goalEase));

            return seq;
        }


        private void UpdateRemainingWords()
        {
            remainingWordsText.text = Round.WordsRemaining.ToString();
            AnimatePunch(remainingWordsText.rectTransform);
        }
        
        private void UpdateDeckAndRedrawCounts()
        {
            deckTilesCountText.text = GameManager.Instance.Deck.DrawPile.Count.ToString();
            redrawCountText.text = Round.DrawsRemaining.ToString();

            AnimatePunch(deckTilesCountText.rectTransform);
            AnimatePunch(redrawCountText.rectTransform);
        }
        #endregion

        #region Animation
        private void AnimateRoundReset(RoundContext context)
        {
            float duration = 1f;
            goalSlider.DOValue(0, duration).SetEase(Ease.OutQuad);

            float animatedValue = goalSlider.value;
            DOTween.To(() => animatedValue, x =>
            {
                animatedValue = x;
                goalPercentageText.text = $"{animatedValue * 100f:0.0}%";
                goalText.text = $"{Mathf.RoundToInt(animatedValue * context.TargetScore)}/{context.TargetScore}";
            }, 0f, duration).SetEase(Ease.OutQuad);

            AnimateNumberText(scoreText, 0, duration);
            AnimateFloatText(modifierText, 1f, duration);
        }
        
        private float AnimateScorePopup(int scoreGain, System.Action onComplete = null)
        {
            if (scoreGain <= 0)
            {
                onComplete?.Invoke();
                return 0;
            }

            // === Instantiation ===
            var popupGO = Instantiate(resultPopupPrefab, popupCanvas.transform);
            var popupText = popupGO.GetComponentInChildren<TextMeshProUGUI>();
            var rect = popupGO.GetComponent<RectTransform>();
            var canvasGroup = popupGO.GetComponent<CanvasGroup>();

            // Position over header (even in another canvas)
            if (goalPercentageText != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    popupCanvas.transform as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(Camera.main, goalPercentageText.transform.position),
                    popupCanvas.worldCamera,
                    out Vector2 anchoredPos
                );
                rect.anchoredPosition = anchoredPos;
            }

            // === Animation Parameters ===
            var duration = Mathf.Lerp(0.8f, 2f, Mathf.Clamp01(scoreGain / 100f)) * 1.5f;
            var punchBase = 0.2f;
            var punchIntensity = Mathf.Lerp(1f, 1.6f, Mathf.Clamp01(scoreGain / 10f));

            // Compute the number of steps
            var steps = Mathf.Clamp(scoreGain, 6, 30); 
            var displayedValue = 0;
            var totalTime = 0f;

            // Curved distribution of step timing
            var distributionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); 
            var seq = DOTween.Sequence();

            for (var i = 0; i < steps; i++)
            {
                var t = (i + 1f) / steps;
                var curvedT = distributionCurve.Evaluate(t);
                var nextTime = curvedT * duration;

                var stepDelay = nextTime - totalTime;
                totalTime = nextTime;

                seq.AppendInterval(Mathf.Max(0f, stepDelay));

                seq.AppendCallback(() =>
                {
                    var targetValue = Mathf.RoundToInt(scoreGain * t);

                    if (targetValue <= displayedValue)
                        return;

                    displayedValue = targetValue;
                    popupText.text = $"+{displayedValue}";

                    popupText.rectTransform.DOKill();
                    popupText.rectTransform.localScale = Vector3.one;
                    popupText.rectTransform
                        .DOPunchScale(Vector3.one * (punchBase * punchIntensity), 0.1f, 8, 1f)
                        .SetEase(Ease.OutQuad);

                    rect.DOShakeAnchorPos(0.12f, 2f * punchIntensity, 10, 90, false, true)
                        .SetEase(Ease.OutSine);
                });
            }
            
            // Final value displayed
            seq.AppendCallback(() =>
            {
                displayedValue = scoreGain;
                popupText.text = $"+{scoreGain}";
            });
            // Some breathing before the fade-out
            seq.AppendInterval(0.5f);

            // Ending => fade-out and cleanup
            seq.Append(canvasGroup.DOFade(0f, 0.4f).SetEase(Ease.OutQuad));
            seq.OnComplete(() =>
            {
                Destroy(popupGO);
                onComplete?.Invoke();
            });

            seq.Play();

            return duration;
        }

        private void AnimateNumberText(TextMeshProUGUI text, int target, float duration)
        {
            int startValue = int.TryParse(text.text, out var parsed) ? parsed : 0;
            DOTween.To(() => startValue, x =>
            {
                startValue = x;
                text.text = startValue.ToString();
            }, target, duration).SetEase(Ease.OutQuad);
        }

        private void AnimateFloatText(TextMeshProUGUI text, float target, float duration)
        {
            if (!float.TryParse(text.text, out float startValue)) startValue = 0f;
            DOTween.To(() => startValue, x =>
            {
                text.text = x.ToString("0.0");
            }, target, duration).SetEase(Ease.OutQuad);
        }

        private void AnimatePunch(RectTransform rect)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy) return;
    
            rect.DOKill();
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity)
                .SetEase(punchEase)
                .SetLink(gameObject); 
        }
        #endregion
    }
}

