using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;

namespace Views
{
    public class SlotView : MonoBehaviour
    {
        [SerializeField] private UIEffect uiEffect;

        #region Mono
        private void Awake()
        {
            uiEffect.enabled = false; 
        }
        #endregion

        public void EnablePreviewFeedback()
        {
            uiEffect.enabled = true;
            uiEffect.edgeWidth = 0;

            var width = 0;
            DOTween.To(() => width, x => width = x, 1, 0.33f)
                .OnUpdate(() => uiEffect.edgeWidth = width);
        }

        public void DisablePreviewFeedback()
        {
            uiEffect.edgeWidth = 1;
            
            var width = 1;
            DOTween.To(() => width, x => width = x, 0, 0.33f)
                .OnUpdate(() => uiEffect.edgeWidth = width)
                .OnComplete(() => uiEffect.enabled = false);
        }
    }
}
