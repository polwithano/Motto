using System.Threading.Tasks;
using Coffee.UIEffects;
using DG.Tweening;
using Events;
using Models;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace Views
{
    public class TileView : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }
        
        [SerializeField] private TextMeshProUGUI characterText;
        [SerializeField] private TextMeshProUGUI valueText;
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

        [field: SerializeField] public Tile Tile       { get; private set; }
        [field: SerializeField] public bool IsInHand   { get; private set; }
        
        public void SetInHand(bool inHand) => IsInHand = inHand;
        
        private Transform _originalParent;
        private Tween _scaleTween; 
        
        #region Mono

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        
        private void OnEnable()
        {
            GameEvents.OnScoreStepStarted += HandleOnTileScored; 
        }

        private void OnDisable()
        {
            GameEvents.OnScoreStepStarted -= HandleOnTileScored;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnTileScored(ScoreLogEntry entry)
        {
            if (entry.Emitter is not Models.Tile) return; 
            if (!Tile.IsInstance(entry.Emitter.ID)) return;

            AnimateOnTileScored();
        }
        #endregion
        
        public void Populate(Tile tile)
        {
            Tile = tile;
            IsInHand = true; 
            characterText.text = tile.Character;
            valueText.text = tile.Points.ToString();
            
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

        public void EndDrag()
        {
            transform.SetParent(_originalParent, true);

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
            var seq = DOTween.Sequence();
            
            var startY = transform.position.y;
            var endY = startY + yOffset;
            
            seq.Append(transform.DOMoveY(endY, duration).SetEase(ease));
            seq.Append(transform.DOMoveY(startY, duration).SetEase(ease));
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
    }
}