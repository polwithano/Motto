using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Events.Core;
using Events.UI;
using UI.Containers.Core;
using UnityEngine;

namespace UI.Containers
{
    [System.Serializable]
    public class UIContainerWrapper
    {
        [field: SerializeField] public UIContainer Container { get; private set; }
        [field: SerializeField] public UIType ContainerType  { get; private set; }
    }
    
    public class UIGame : MonoBehaviourSingleton<UIGame>
    {
        [SerializeField] private List<UIContainerWrapper> containers = new ();
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [field: SerializeField] public UIRoundContext RoundContext { get; private set; }

        [Header("Tween Settings")]
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;
        
        #region Mono
        private void OnEnable()
        {
            Bus<SetUIContainerStateEvent>.OnEvent += HandleOnUIContainerStateUpdated; 
        }

        private void OnDisable()
        {
            Bus<SetUIContainerStateEvent>.OnEvent -= HandleOnUIContainerStateUpdated; 
        }
        
        private void OnDestroy() => OnDisable(); 
        #endregion
        
        #region Event Handlers
        private void HandleOnUIContainerStateUpdated(SetUIContainerStateEvent evt)
        {
            foreach (var container in containers.Where(container => container.ContainerType == evt.Container))
            {
                container.Container.UpdateContainerState(evt.State);
            }
            FadeOverlay(evt.State);
        }

        private void FadeOverlay(UIState state)
        {
            var active = state == UIState.Opened;
            var endValue = state == UIState.Opened ? 1f : 0f; 
            
            overlayCanvasGroup.DOFade(endValue, fadeDuration).SetEase(fadeEase);
            overlayCanvasGroup.interactable = active;
            overlayCanvasGroup.blocksRaycasts = active;
        }
        #endregion
    }
}