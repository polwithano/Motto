using System.Collections.Generic;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Shop;
using Managers;
using TMPro;
using UnityEngine;
using Views;

namespace UI
{
    public class UIShop : MonoBehaviour
    {
        [SerializeField] private GameObject tileViewPrefab;
        [SerializeField] private GameObject tileShopContainerPrefab; 
        [SerializeField] private RectTransform tileContainer;
        
        [SerializeField] private TextMeshProUGUI nextRoundLabel;
        [SerializeField] private TextMeshProUGUI rerollLabel; 
        
        [SerializeField] private List<GameObject>  shopTiles;
        
        #region Mono
        private void OnEnable()
        {
            Bus<ShopStatusEvent>.OnEvent += HandleShopStatusUpdated; 
        }

        private void OnDisable()
        {
            Bus<ShopStatusEvent>.OnEvent -= HandleShopStatusUpdated; 

        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Event Handlers
        private void HandleShopStatusUpdated(ShopStatusEvent evt)
        {
            switch (evt.Status)
            {
                case ShopStatus.Open:
                    HandleOnShopOpen();
                    break; 
                case ShopStatus.Closed:
                    HandleOnShopClosed();
                    break; 
                case ShopStatus.Reroll:
                    UpdateStaticUI();
                    InstantiateShopItemViews();
                    break;
            }
        }
        
        private void HandleOnShopOpen()
        {
            UpdateStaticUI();
            InstantiateShopItemViews();
        }

        private void HandleOnShopClosed()
        { 
            ClearShopItemViews();
        }
        #endregion

        public void OnNextRoundButton() => Bus<ShopStatusEvent>.Raise(new ShopStatusEvent(ShopStatus.Closed));

        public void OnRerollButton()
        {
           Bus<ShopStatusEvent>.Raise(new ShopStatusEvent(ShopStatus.Reroll));
        }

        private void UpdateStaticUI()
        {
            var run = GameManager.Instance.Run; 
            var nextRoundText = $"{run.Round.Definition.RoundType} ({run.RoundIndex + 1})";
            var rerollValueText = $"${ShopManager.Instance.RerollPrice}"; 
            
            nextRoundLabel.text = nextRoundText;
            rerollLabel.text = rerollValueText;
        }
        
        private void InstantiateShopItemViews()
        {
            ClearShopItemViews();
            
            var tiles = ShopManager.Instance.Tiles;
            var delay = 0f; 
            
            foreach (var tile in tiles)
            {
                var tileShopContainer = Instantiate(tileShopContainerPrefab, tileContainer);
                var tileView = Instantiate(tileViewPrefab, tileShopContainer.transform);
                var view = tileView.GetComponent<TileView>();

                shopTiles.Add(tileShopContainer);
                tileView.transform.SetAsFirstSibling();
                view.Populate(tile);
                
                var t = tileView.transform;
                t.localScale = Vector3.zero;

                t.DOScale(1.33f, 0.15f)
                    .SetEase(Ease.OutBounce)
                    .SetDelay(delay)
                    .OnComplete(() => t.DOScale(1f, 0.15f).SetEase(Ease.InOutSine));

                delay += 0.033f; 
                
                Destroy(view);
            }
        }

        private void ClearShopItemViews()
        {
            foreach (var obj in shopTiles) Destroy(obj);
            shopTiles.Clear();
        }
    }
}