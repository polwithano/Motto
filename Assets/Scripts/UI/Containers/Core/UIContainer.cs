using Events.UI;
using UnityEngine;

namespace UI.Containers.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIContainer : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        protected virtual void RegisterEvents() {}
        protected virtual void UnregisterEvents() {}

        protected virtual void Open()
        {
            RegisterEvents();
            
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        protected virtual void Close()
        {
            UnregisterEvents();
            
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        public void UpdateContainerState(UIState state)
        {
            switch (state)
            {
                case UIState.Opened:
                    Open();
                    break;
                case UIState.Closed:
                    Close();
                    break;
            } 
        }
    }
}