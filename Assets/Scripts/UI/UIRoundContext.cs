using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Events;
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
            GameEvents.OnRoundStarted += HandleOnRoundStarted;
            GameEvents.OnScoringStarted += HandleOnScoringStarted; 
            GameEvents.OnScoreStepStarted += HandleOnScoreStepStarted;
            GameEvents.OnTileRedrawPerformed += HandleOnTileRedrawPerformed;
            GameEvents.OnPurchaseProcessed += HandleOnPurchaseProcessed;
        }

        private void OnDisable()
        {
            GameEvents.OnRoundStarted -= HandleOnRoundStarted;
            GameEvents.OnScoringStarted -= HandleOnScoringStarted; 
            GameEvents.OnScoreStepStarted -= HandleOnScoreStepStarted;
            GameEvents.OnTileRedrawPerformed -= HandleOnTileRedrawPerformed; 
            GameEvents.OnPurchaseProcessed -= HandleOnPurchaseProcessed;
        }

        private void OnDestroy() => OnDisable();
        #endregion

        #region Event Handlers
        private void HandleOnRoundStarted(RoundContext context)
        {
            UpdateStaticRoundData();
            AnimateRoundReset(context);
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


        private void HandleOnTileRedrawPerformed(Tile tile, Tile newTile)
        {
            UpdateDeckAndRedrawCounts();
        }

        private void HandleOnPurchaseProcessed()
        {
            currencyCountText.text = $"${GameManager.Instance.Run.Currency.ToString()}"; 
            AnimatePunch(currencyCountText.rectTransform);
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

            // Instantiate popup
            var popupGO = Instantiate(resultPopupPrefab, popupCanvas.transform);
            var popupText = popupGO.GetComponentInChildren<TextMeshProUGUI>();
            var rect = popupGO.GetComponent<RectTransform>();
            var canvasGroup = popupGO.GetComponent<CanvasGroup>();

            // ✅ Position over header (even in another canvas)
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
            float duration = Mathf.Lerp(0.8f, 2f, Mathf.Clamp01(scoreGain / 100f)) * 1.5f;
            float punchBase = 0.2f;
            float punchIntensity = Mathf.Lerp(1f, 1.6f, Mathf.Clamp01(scoreGain / 10f));

            // Steps
            int steps = Mathf.Clamp(Mathf.RoundToInt(Mathf.Log10(scoreGain + 10) * 15), 8, 30);
            int displayedValue = 0;
            int increment = Mathf.Max(1, scoreGain / steps);

            // Curved distribution of step timing
            AnimationCurve distributionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); 
            float totalTime = 0f;

            // Build the sequence
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < steps; i++)
            {
                // Curve-based step spacing
                float t = (i + 1f) / steps;
                float curvedT = distributionCurve.Evaluate(t);
                float nextTime = curvedT * duration;

                // Delta since last step
                float stepDelay = nextTime - totalTime;
                totalTime = nextTime;

                // Add ±10% randomness to timing
                stepDelay *= Random.Range(0.9f, 1.1f);

                // Schedule step
                seq.AppendInterval(Mathf.Max(0f, stepDelay));
                seq.AppendCallback(() =>
                {
                    displayedValue = Mathf.Min(displayedValue + increment, scoreGain);
                    popupText.text = $"+{displayedValue}";

                    // --- Punch scale ---
                    popupText.rectTransform.DOKill();
                    popupText.rectTransform.localScale = Vector3.one;
                    popupText.rectTransform
                        .DOPunchScale(Vector3.one * (punchBase * punchIntensity), 0.1f, 8, 1f)
                        .SetEase(Ease.OutQuad);

                    // --- Optional subtle shake (position jitter) ---
                    rect.DOShakeAnchorPos(0.12f, 2f * punchIntensity, 10, 90, false, true)
                        .SetEase(Ease.OutSine);
                });
            }

            // End: fade out and cleanup
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
                .SetLink(gameObject); // <— ensures safe cleanup
        }
        #endregion
    }
}

