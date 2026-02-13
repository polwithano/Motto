using System.Collections.Generic;
using DG.Tweening;
using Events.Core;
using Events.Shop;
using Managers;
using Models;
using Models.Charms.Core;
using TMPro;
using UI.Containers.Core;
using UnityEngine;
using Views;

namespace UI.Containers
{
    public class UIShop : UIContainer
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject charmPrefab; 
        [SerializeField] private IBuyableViewer buyableViewerPrefab;

        [Header("Layout")]
        [SerializeField] private RectTransform tilesContainerRoot;
        [SerializeField] private RectTransform charmsContainerRoot;

        [Header("Static UI")]
        [SerializeField] private TextMeshProUGUI nextRoundLabel;
        [SerializeField] private TextMeshProUGUI rerollLabel;

        private List<IBuyableViewer> _buyableViewers = new();

        #region UI Container
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            Bus<ShopInventoryUpdatedEvent>.OnEvent += HandleShopRefresh; 
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            Bus<ShopInventoryUpdatedEvent>.OnEvent -= HandleShopRefresh; 
        }
        #endregion

        #region Events
        private void HandleShopRefresh(ShopInventoryUpdatedEvent args)
        {
            RefreshShop();
        }
        #endregion

        #region UI Flow
        protected override void Open()
        {
            base.Open();
            UpdateStaticUI();
            SpawnShopBundleViews();
        }

        private void RefreshShop()
        {
            UpdateStaticUI();
            SpawnShopBundleViews();
        }

        protected override void Close()
        {
            base.Close();
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
            SpawnCharmBundleViews();
            SpawnTileBundleViews();
        }

        private void SpawnCharmBundleViews()
        {
            var delay = 0f;

            foreach (var bundle in ShopManager.Instance.Charms)
            {
                var buyable = Instantiate(buyableViewerPrefab, charmsContainerRoot);
                var charm = Instantiate(charmPrefab, buyable.transform);
                charm.transform.SetAsFirstSibling();
                
                var view = charm.GetComponent<CharmView>(); 
                
                view.Populate(bundle.Item as Charm);
                buyable.Initialize(bundle);
                
                AnimateAppear(charm.transform, delay);
                delay += 0.03f;

                _buyableViewers.Add(buyable);
            }
        }
        
        private void SpawnTileBundleViews()
        {
            var delay = 0f;

            foreach (var bundle in ShopManager.Instance.Tiles)
            {
                var buyable = Instantiate(buyableViewerPrefab, tilesContainerRoot);
                var tile = Instantiate(tilePrefab, buyable.transform);
                tile.transform.SetAsFirstSibling();
                
                var view = tile.GetComponent<TileView>(); 
                
                view.Populate(bundle.Item as Tile);
                buyable.Initialize(bundle);

                Destroy(view);
                
                AnimateAppear(tile.transform, delay);
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
        public void OnRerollClicked()
        {
            Bus<ShopRerollRequestEvent>.Raise(new ShopRerollRequestEvent());
        }
        #endregion
    }
}
