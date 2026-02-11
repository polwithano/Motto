using System.Collections.Generic;
using DG.Tweening;
using Events.Core;
using Events.Shop;
using UnityEngine;

namespace UI.Containers
{
    public class UIGame : MonoBehaviourSingleton<UIGame>
    {
        [SerializeField] private List<CanvasGroup> gameCanvases; 
        [SerializeField] private CanvasGroup shopCanvas;
        [field: SerializeField] public UIRoundContext RoundContext { get; private set; }

        [Header("Tween Settings")]
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;
        
        #region Mono
        private void Start()
        {
            SetCanvasGroupsActive(gameCanvases, true);
            SetCanvasGroupActive(shopCanvas, false);
        }
        
        private void OnEnable()
        {
            Bus<ShopStateEvent>.OnEvent += HandleOnShopStatusUpdated; 
        }

        private void OnDisable()
        {
            Bus<ShopStateEvent>.OnEvent -= HandleOnShopStatusUpdated; 
        }

        private void OnDestroy() => OnDisable(); 
        #endregion
        
        #region Event Handlers
        private void HandleOnShopStatusUpdated(ShopStateEvent evt)
        {
            switch (evt.State)
            {
                case ShopState.Opened:
                    HandleOnShopOpened();
                    break; 
                case ShopState.Closed:
                    HandleOnShopClosed();
                    break; 
            }
        }
        
        private void HandleOnShopOpened()
        {
            FadeCanvasGroups(gameCanvases, false);
            FadeCanvasGroup(shopCanvas, true);
        }

        private void HandleOnShopClosed()
        {
            FadeCanvasGroups(gameCanvases, true);
            FadeCanvasGroup(shopCanvas, false);
        }
        #endregion
        
        private void FadeCanvasGroup(CanvasGroup group, bool fadeIn)
        {
            if (group == null) return;
            
            float targetAlpha = fadeIn ? 1f : 0f;
            group.DOKill();
            group.DOFade(targetAlpha, fadeDuration)
                .SetEase(fadeEase);

            group.interactable = fadeIn;
            group.blocksRaycasts = fadeIn;
        }

        private void FadeCanvasGroups(IEnumerable<CanvasGroup> groups, bool fadeIn)
        {
            if (groups == null) return;
            foreach (var group in groups)
                FadeCanvasGroup(group, fadeIn);
        }
        
        private void SetCanvasGroupActive(CanvasGroup group, bool active)
        {
            if (group == null) return;
            group.DOKill(); // cancel tweens if any
            group.alpha = active ? 1f : 0f;
            group.interactable = active;
            group.blocksRaycasts = active;
        }

        private void SetCanvasGroupsActive(IEnumerable<CanvasGroup> groups, bool active)
        {
            if (groups == null) return;
            foreach (var group in groups)
                SetCanvasGroupActive(group, active);
        }
    }
}