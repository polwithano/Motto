using System.Collections.Generic;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Shop;
using Managers;
using Models;
using TMPro;
using UnityEngine;
using Views;

namespace UI
{
    public class UIShop : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject itemContainerPrefab;
        [SerializeField] private IBuyableViewer buyableViewerPrefab;

        [Header("Layout")]
        [SerializeField] private RectTransform itemsRoot;

        [Header("Static UI")]
        [SerializeField] private TextMeshProUGUI nextRoundLabel;
        [SerializeField] private TextMeshProUGUI rerollLabel;

        private List<IBuyableViewer> _buyableViewers = new();

        #region Mono
        private void OnEnable()
        {
            Bus<ShopStateEvent>.OnEvent += HandleShopStatus;
            Bus<ShopInventoryUpdatedEvent>.OnEvent += HandleShopRefresh; 
        }

        private void OnDisable()
        {
            Bus<ShopStateEvent>.OnEvent -= HandleShopStatus;
            Bus<ShopInventoryUpdatedEvent>.OnEvent -= HandleShopRefresh; 
        }
        #endregion

        #region Events
        private void HandleShopStatus(ShopStateEvent evt)
        {
            switch (evt.State)
            {
                case ShopState.Opened:
                    OpenShop();
                    break;
                
                case ShopState.Closed:
                    CloseShop();
                    break;
            }
        }
        
        private void HandleShopRefresh(ShopInventoryUpdatedEvent args)
        {
            RefreshShop();
        }
        #endregion

        #region UI Flow
        private void OpenShop()
        {
            UpdateStaticUI();
            SpawnShopBundleViews();
        }

        private void RefreshShop()
        {
            UpdateStaticUI();
            SpawnShopBundleViews();
        }

        private void CloseShop()
        {
            ClearBundleViews();
        }
        #endregion

        #region UI Helpers
        private void UpdateStaticUI()
        {
            var run = GameManager.Instance.Run;
            
            nextRoundLabel.text = $"{run.Round.Definition.RoundType} ({run.RoundIndex + 1})";
            rerollLabel.text = $"${ShopManager.Instance.RerollPrice}";
        }

        private void SpawnShopBundleViews()
        {
            ClearBundleViews();

            var delay = 0f;

            foreach (var bundle in ShopManager.Instance.ShopItems)
            {
                var buyable = Instantiate(buyableViewerPrefab, itemsRoot);
                var container = Instantiate(itemContainerPrefab, buyable.transform);
                container.transform.SetAsFirstSibling();
                
                var view = container.GetComponent<TileView>(); 
                
                view.Populate(bundle.Item as Tile);
                buyable.Initialize(bundle);

                Destroy(view);
                
                AnimateAppear(container.transform, delay);
                delay += 0.03f;

                _buyableViewers.Add(buyable);
            }
        }

        private void ClearBundleViews()
        {
            foreach (var obj in _buyableViewers)
                Destroy(obj.gameObject);

            _buyableViewers.Clear();
        }

        private static void AnimateAppear(Transform t, float delay)
        {
            t.localScale = Vector3.zero;

            t.DOScale(1.15f, 0.15f)
                .SetEase(Ease.OutBack)
                .SetDelay(delay)
                .OnComplete(() =>
                    t.DOScale(1f, 0.1f).SetEase(Ease.InOutSine));
        }
        #endregion

        #region Buttons
        public void OnNextRoundClicked()
        {
            Bus<ShopStateEvent>.Raise(new ShopStateEvent(ShopState.Closed));
        }

        public void OnRerollClicked()
        {
            Bus<ShopRerollRequestEvent>.Raise(new ShopRerollRequestEvent());
        }
        #endregion
    }
}
