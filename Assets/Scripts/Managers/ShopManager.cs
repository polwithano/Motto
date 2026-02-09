using System.Collections.Generic;
using Events.Core;
using Events.Shop;
using Models;
using Models.Shop;
using UnityEngine;

namespace Managers
{
    public class ShopManager : MonoBehaviourSingleton<ShopManager>
    {
        [Header("Pools")]
        [SerializeField] private List<TileModifierSO> tileModifiersPool;

        [Header("Settings")]
        [SerializeField] private int defaultRerollPrice = 5;
        [SerializeField] private int shopItemCount = 4;

        public IReadOnlyList<ShopItemBundle> ShopItems => _shopItems;
        public int RerollPrice { get; private set; }

        private readonly List<ShopItemBundle> _shopItems = new();

        #region Public API
        public void InitializeShop()
        {
            RerollPrice = defaultRerollPrice;
            
            InitializeShopItemBundles();
        }

        public void RerollShop()
        {
            RerollPrice = Mathf.Min(RerollPrice *= defaultRerollPrice, 1000);
            
            InitializeShopItemBundles();
        }

        public void RemoveBundle(ShopItemBundle bundle)
        {
            _shopItems.Remove(bundle);
        }
        #endregion

        #region Internals
        private void InitializeShopItemBundles()
        {
            _shopItems.Clear();

            // Allowed characters that will appear in the shop. 
            var allowedCharacters = GameManager.Instance.TileDistributionRule.CharacterRules;

            for (var i = 0; i < shopItemCount; i++)
            {
                var character = allowedCharacters[Random.Range(0, allowedCharacters.Count)];
                var modifier = tileModifiersPool[Random.Range(0, tileModifiersPool.Count)];

                var tile = new Tile(character.character, character.pointValue, character.isBlank, modifier);
                var price = (uint)Mathf.FloorToInt(tile.Points * modifier.PriceModifier);
                tile.SetPrice(price);

                _shopItems.Add(new ShopItemBundle(tile));
            }
        }
        #endregion
    }
}