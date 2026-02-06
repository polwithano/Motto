using System.Collections.Generic;
using DG.Tweening;
using Events;
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
            GameEvents.OnShopOpened += HandleOnShopOpen; 
            GameEvents.OnShopClosed += HandleOnShopClosed;
        }

        private void OnDisable()
        {
            GameEvents.OnShopOpened -= HandleOnShopOpen;
            GameEvents.OnShopClosed -= HandleOnShopClosed;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Event Handlers
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

        public void OnNextRoundButton() => GameEvents.RaiseOnShopClosed();
        public void OnRerollButton()
        {
            GameEvents.RaiseOnShopReroll();
            
            UpdateStaticUI();
            InstantiateShopItemViews(); 
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