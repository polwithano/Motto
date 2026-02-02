using Coffee.UIEffects;
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
        }

        public void DisablePreviewFeedback()
        {
            uiEffect.enabled = false;
        }
    }
}
