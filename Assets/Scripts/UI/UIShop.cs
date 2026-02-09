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

        private readonly List<GameObject> _spawnedItems = new();

        #region Mono
        private void OnEnable()
        {
            Bus<ShopStatusEvent>.OnEvent += HandleShopStatus;
        }

        private void OnDisable()
        {
            Bus<ShopStatusEvent>.OnEvent -= HandleShopStatus;
        }
        #endregion

        #region Events
        private void HandleShopStatus(ShopStatusEvent evt)
        {
            switch (evt.Status)
            {
                case ShopStatus.Open:
                    OpenShop();
                    break;

                case ShopStatus.Reroll:
                    RefreshShop();
                    break;

                case ShopStatus.Closed:
                    CloseShop();
                    break;
            }
        }
        #endregion

        #region UI Flow
        private void OpenShop()
        {
            UpdateStaticUI();
            SpawnItems();
        }

        private void RefreshShop()
        {
            UpdateStaticUI();
            SpawnItems();
        }

        private void CloseShop()
        {
            ClearItems();
        }
        #endregion

        #region UI Helpers
        private void UpdateStaticUI()
        {
            var run = GameManager.Instance.Run;
            nextRoundLabel.text = $"{run.Round.Definition.RoundType} ({run.RoundIndex + 1})";
            rerollLabel.text = $"${ShopManager.Instance.RerollPrice}";
        }

        private void SpawnItems()
        {
            ClearItems();

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

                _spawnedItems.Add(buyable.gameObject);
            }
        }

        private void ClearItems()
        {
            foreach (var obj in _spawnedItems)
                Destroy(obj);

            _spawnedItems.Clear();
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
            Bus<ShopStatusEvent>.Raise(new ShopStatusEvent(ShopStatus.Closed));
        }

        public void OnRerollClicked()
        {
            Bus<ShopStatusEvent>.Raise(new ShopStatusEvent(ShopStatus.Reroll));
        }
        #endregion
    }
}
