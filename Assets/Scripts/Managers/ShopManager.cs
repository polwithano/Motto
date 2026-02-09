using System.Collections.Generic;
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
        [SerializeField] private int defaultRerollPrice = 10;
        [SerializeField] private int shopItemCount = 4;

        public IReadOnlyList<ShopItemBundle> ShopItems => _shopItems;
        public int RerollPrice { get; private set; }

        private readonly List<ShopItemBundle> _shopItems = new();

        #region Public API
        public void InitializeShop()
        {
            RerollPrice = defaultRerollPrice;
            RegenerateShop();
        }

        public void RerollShop()
        {
            RerollPrice += defaultRerollPrice;
            RegenerateShop();
        }
        #endregion

        #region Internals
        private void RegenerateShop()
        {
            _shopItems.Clear();

            var rules = GameManager.Instance.TileDistributionRule.CharacterRules;

            for (int i = 0; i < shopItemCount; i++)
            {
                var rule = rules[Random.Range(0, rules.Count)];
                var modifier = tileModifiersPool[Random.Range(0, tileModifiersPool.Count)];

                var tile = new Tile(
                    rule.character,
                    rule.pointValue,
                    rule.isBlank,
                    modifier
                );

                var price = Mathf.FloorToInt(tile.Points * modifier.PriceModifier);

                _shopItems.Add(new ShopItemBundle(tile, price));
            }
        }
        #endregion
    }
}