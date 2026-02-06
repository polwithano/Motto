using Coffee.UIEffects;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Rounds;
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
        [SerializeField] private UIEffect charmEffect;
        
        [Header("Animation Settings")]
        [SerializeField] private float punchScaleAmount = 0.2f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private int vibrato = 6;
        [SerializeField] private float elasticity = 0.8f;
        [SerializeField] private Ease ease = Ease.OutBack;

        [Header("Preview Settings")] 
        [SerializeField] [ColorUsage(true, true)] private Color disabledColor;
        [SerializeField] [ColorUsage(true, true)] private Color disabledColorOutline;
        [SerializeField] [ColorUsage(true, true)] private Color enabledColor;
        [SerializeField] [ColorUsage(true, true)] private Color enabledColorOutline;
    
        private Tween _punchTween;
        
        #region Mono
        private void OnEnable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted; 
            GameEvents.OnScoreStepStarted += HandleOnScoreStepStarted;
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent -= HandleOnRoundStarted; 
            GameEvents.OnScoreStepStarted -= HandleOnScoreStepStarted;
        }

        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnRoundStarted(RoundStartedEvent evt)
        {
            SetActiveFeedback(false);
        }
        
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

        public void SetActiveFeedback(bool active)
        {
            charmIcon.DOKill();
            DOTween.Kill(charmEffect);

            var targetColor = active ? enabledColor : disabledColor;
            var targetOutline = active ? enabledColorOutline : disabledColorOutline;

            charmIcon
                .DOColor(targetColor, 0.25f)
                .SetEase(Ease.OutCubic);

            DOTween.To(
                () => charmEffect.shadowColor,
                c => charmEffect.shadowColor = c,
                targetOutline,
                0.25f
            ).SetEase(Ease.OutCubic);
        }

        private void AnimateCharm()
        {
            _punchTween?.Kill(); 

            var target = charmIcon.transform;
            var seq = DOTween.Sequence();
            
            seq.Append(target.DOPunchScale(Vector3.one * punchScaleAmount, punchDuration, vibrato, elasticity));
            seq.Join(target.DOLocalRotate(new Vector3(0, 0, Random.Range(-10f, 10f)), punchDuration * 0.5f).SetEase(ease));
            seq.Append(target.DOLocalRotate(Vector3.zero, punchDuration * 0.5f).SetEase(ease));
        }
    }
}
