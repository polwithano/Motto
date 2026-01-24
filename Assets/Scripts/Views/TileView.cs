using DG.Tweening;
using Events;
using Models;
using UnityEngine;
using TMPro; 

namespace Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI characterText;
        [SerializeField] private TextMeshProUGUI valueText;

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
        
        #region Mono
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
            
            PopulateTileModifier();
        }
        
        public void OnTileRedraw()
        {
            AnimatePunch(GetComponent<RectTransform>());
        }

        private void PopulateTileModifier()
        {
            if (Tile.Modifier != null)
            {
                characterText.font = Tile.Modifier.FontAsset; 
            }
        }

        private void AnimateOnTileScored()
        {
            Sequence seq = DOTween.Sequence();
            
            float startY = transform.position.y;
            float endY = startY + yOffset;
            
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
                .SetLink(gameObject); // <— ensures safe cleanup
        }
    }
}