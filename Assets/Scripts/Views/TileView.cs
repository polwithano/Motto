using System.Threading.Tasks;
using Coffee.UIEffects;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Score;
using Models;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Views
{
    public class TileView : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }
        
        [SerializeField] private TextMeshProUGUI characterText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private RawImage blankIcon; 
        [SerializeField] private UIEffect uiEffect;
        [SerializeField] private UIEffectTweener uiEffectTweener;

        [Header("Animation")] 
        [SerializeField] private float yOffset;
        [SerializeField] private float duration; 
        [SerializeField] private Ease ease; 
        
        [Header("Animation Settings")]
        [SerializeField] private float punchScale = 0.25f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private int punchVibrato = 6;
        [SerializeField] private float punchElasticity = 1f;
        [SerializeField] private Ease punchEase = Ease.OutBack;

        [Header("State")] 
        [SerializeField] private Color inactiveStateColor; 

        [field: SerializeField] public Tile Tile       { get; private set; }
        [field: SerializeField] public bool IsInHand   { get; private set; }
        
        public void SetInHand(bool inHand) => IsInHand = inHand;
        
        private CanvasGroup _canvasGroup;
        private Transform _originalParent;
        private Image _image;
        private Tween _scaleTween; 
        private Tween _stateTween;
        
        #region Mono
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            
            _canvasGroup = GetComponent<CanvasGroup>();
            _image = GetComponent<Image>();
        }
        
        private void OnEnable()
        {
            Bus<ScoringStepStartedEvent>.OnEvent += HandleOnTileScored;
            Bus<ScoringSequenceStartedEvent>.OnEvent += HandleScoringSequenceStarted;
            Bus<ScoringSequenceOverEvent>.OnEvent += HandleScoringSequenceOver; 
        }
        
        private void OnDisable()
        {
            Bus<ScoringStepStartedEvent>.OnEvent -= HandleOnTileScored;
            Bus<ScoringSequenceStartedEvent>.OnEvent -= HandleScoringSequenceStarted;
            Bus<ScoringSequenceOverEvent>.OnEvent -= HandleScoringSequenceOver; 
        }
        #endregion
        
        #region Subscribed
        private void HandleOnTileScored(ScoringStepStartedEvent evt)
        {
            if (evt.Entry.Emitter is not Models.Tile) return; 
            if (!Tile.IsInstance(evt.Entry.Emitter.ID)) return;

            AnimateOnTileScored();
        }
        
        private void HandleScoringSequenceStarted(ScoringSequenceStartedEvent evt)
        {
            SetActiveState(false);
        }
        
        private void HandleScoringSequenceOver(ScoringSequenceOverEvent evt)
        {
            SetActiveState(true);
        }
        #endregion
        
        public void Populate(Tile tile)
        {
            Tile = tile;
            IsInHand = true; 
            valueText.text = tile.Points.ToString();

            if (tile.IsBlank)
            {
                blankIcon.enabled = true;
                characterText.enabled = false;
            }
            else
            {
                blankIcon.enabled = false;
                characterText.text = tile.Character.ToString();
            }
            
            gameObject.name = $"Tile_{tile.Character}";
            
            PopulateTileModifier();
        }
        
        private void PopulateTileModifier()
        {
            if (Tile.Modifier != null)
            {
                characterText.font = Tile.Modifier.FontAsset; 
            }
        }

        public void BeginDrag(Transform dragLayer)
        {
            _originalParent = transform.parent;
            transform.SetParent(dragLayer, true);
            
            RectTransform.SetAsLastSibling();
            _scaleTween?.Kill();
            _scaleTween = RectTransform
                .DOScale(Vector3.one * 1.15f, 0.15f)
                .SetEase(Ease.OutBack);
        }
        
        public void BeginFreeMove(Transform animationLayer)
        {
            _originalParent = transform.parent;
            transform.SetParent(animationLayer, true);
            RectTransform.SetAsLastSibling();
        }

        public void EndDrag()
        {
            _scaleTween?.Kill();
            RectTransform.localScale = Vector3.one;
        }
        
        public void OnTileRedraw()
        {
            AnimatePunch(GetComponent<RectTransform>());
        }

        public Task AnimateOnTileSlotClearedAsync(int index)
        {
            var tcs = new TaskCompletionSource<bool>();

            UnityAction onComplete = null;

            onComplete = () =>
            {
                uiEffectTweener.onComplete.RemoveListener(onComplete);
                tcs.TrySetResult(true);
            };

            uiEffectTweener.onComplete.AddListener(onComplete);

            var randomRotation = Random.Range(0f, 360f);
            uiEffect.transitionRotation = randomRotation;

            uiEffectTweener.delay = index * .25f; 
            uiEffectTweener.Play(true);

            return tcs.Task;
        }
        
        private void AnimateOnTileScored()
        {
            transform.DOKill();

            var seq = DOTween.Sequence();

            // Random left/right rotation
            var randomAngle = Random.Range(15f, 30f);
            var direction = Random.value > 0.5f ? 1f : -1f;
            var targetAngle = randomAngle * direction;

            // Rotate
            seq.Join(
                transform.DORotate(
                    new Vector3(0, 0, targetAngle),
                    duration * 0.5f
                ).SetEase(ease)
            );

            // Back to the initial rotation
            seq.Append(
                transform.DORotate(
                    Vector3.zero,
                    duration * 0.5f
                ).SetEase(ease)
            );

            // Punch scale
            seq.Join(
                transform.DOPunchScale(
                    Vector3.one * 0.275f,  
                    duration,
                    8,                    
                    0.8f               
                )
            );
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

        private void SetActiveState(bool isActive)
        {
            _stateTween?.Kill();
            
            // We don't visually fade the tiles that are on the board. 
            if (!IsInHand) return;
            
            var targetAlpha = isActive ? 1f : 0.85f;
            var targetColor = isActive ? Color.white : inactiveStateColor;

            var seq = DOTween.Sequence();

            // Fade
            seq.Join(
                _canvasGroup.DOFade(targetAlpha, 0.25f)
            );

            // Color tint
            seq.Join(
                _image.DOColor(targetColor, 0.25f)
            );

            _stateTween = seq;
        }
    }
}