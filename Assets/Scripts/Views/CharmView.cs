using DG.Tweening;
using Events;
using Models;
using Models.Charms;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class CharmView : MonoBehaviour
    {
        [field: SerializeField] public Charm Charm { get; private set; }
    
        [SerializeField] private Image charmIcon;
        [SerializeField] private Image charmIconShadow;
        
        [Header("Animation Settings")]
        [SerializeField] private float punchScaleAmount = 0.2f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private int vibrato = 6;
        [SerializeField] private float elasticity = 0.8f;
        [SerializeField] private Ease ease = Ease.OutBack;
    
        private Tween _punchTween;
        
        #region Mono
        private void OnEnable()
        {
            GameEvents.OnScoreStepStarted += HandleOnScoreStepStarted; 
        }

        private void OnDisable()
        {
            GameEvents.OnScoreStepStarted -= HandleOnScoreStepStarted;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnScoreStepStarted(ScoreLogEntry entry)
        {
            if (entry.Emitter is not Charm charm) return;
            if (!charm.IsInstance(Charm.ID)) return; 

            AnimateCharm();
        }
        #endregion
        
        public void Populate(Charm charm)
        {
            Charm =  charm;
            charmIcon.sprite = Charm.CharmIcon;
            charmIconShadow.sprite = Charm.CharmIcon; 
        }

        private void AnimateCharm()
        {
            _punchTween?.Kill(); // avoid stacking tweens

            // Combine scale + small rotation for a bit more life
            var target = charmIcon.transform;
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOPunchScale(Vector3.one * punchScaleAmount, punchDuration, vibrato, elasticity));
            seq.Join(target.DOLocalRotate(new Vector3(0, 0, Random.Range(-10f, 10f)), punchDuration * 0.5f).SetEase(ease));
            seq.Append(target.DOLocalRotate(Vector3.zero, punchDuration * 0.5f).SetEase(ease));
        }
    }
}
