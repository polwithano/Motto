using System.Collections.Generic;
using System.Linq;
using Models;
using Models.Charms;
using Models.Charms.Core;
using Models.Shop;
using Models.SO;
using UnityEngine;

namespace Managers
{
    public class ShopManager : MonoBehaviourSingleton<ShopManager>
    {
        [Header("Pools")]
        [SerializeField] private List<TileModifierSO> tileModifiersPool;

        [Header("Settings")]
        [SerializeField] private uint defaultRerollPrice = 5;
        [SerializeField] private int tileItemsCount = 4;
        [SerializeField] private int charmItemsCount = 4;

        public IReadOnlyList<ShopItemBundle> Tiles => _shopItems.Where(x => x.Item is Tile).ToList();
        public IReadOnlyList<ShopItemBundle> Charms => _shopItems.Where(x => x.Item is Charm).ToList();
        
        public uint RerollPrice { get; private set; }

        private readonly List<ShopItemBundle> _shopItems = new();

        #region Public API
        public void InitializeShop()
        {
            RerollPrice = defaultRerollPrice;
            
            InitializeShopItemBundles();
        }

        public void RerollShop()
        {
            RerollPrice = (uint)Mathf.FloorToInt(Mathf.Min(RerollPrice *= defaultRerollPrice, 1000));
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

            // Tiles Shop Bundles
            // Allowed characters that will appear in the shop. 
            var allowedCharacters = GameManager.Instance.TileDistributionRule.CharacterRules;
            _shopItems.AddRange(InitializeTilesItemBundles(allowedCharacters, tileItemsCount));
            
            // Charms Shop Bundles
            var allowedCharms = CharmManager.Instance.GetNonActiveCharmsFromDatabase(GameManager.Instance.CharmsDatabase);
            _shopItems.AddRange(InitializeCharmsItemBundles(allowedCharms, charmItemsCount));
        }

        private List<ShopItemBundle> InitializeTilesItemBundles(
            List<TileDistributionRuleSO.CharacterRule> allowedCharacters, 
            int tilesCount)
        {
            var bundles = new List<ShopItemBundle>();
            
            for (var i = 0; i < tilesCount; i++)
            {
                var character = allowedCharacters[Random.Range(0, allowedCharacters.Count)];
                var modifier = tileModifiersPool[Random.Range(0, tileModifiersPool.Count)];

                var tile = new Tile(character.character, character.pointValue, character.isBlank, modifier);
                var price = (uint)Mathf.FloorToInt(tile.Points * modifier.PriceModifier);
                tile.SetPrice(price);

                bundles.Add(new ShopItemBundle(tile));
            }
            
            return bundles;
        }

        private List<ShopItemBundle> InitializeCharmsItemBundles(
            List<Charm> allowedCharms,
            int charmsCount)
        {
            var bundles = new List<ShopItemBundle>();

            if (allowedCharms == null || allowedCharms.Count == 0 || charmsCount <= 0)
                return bundles;

            var pool = new List<Charm>(allowedCharms);

            for (var i = 0; i < pool.Count; i++)
            {
                var rand = Random.Range(i, pool.Count);
                (pool[i], pool[rand]) = (pool[rand], pool[i]);
            }

            var count = Mathf.Min(charmsCount, pool.Count);

            for (var i = 0; i < count; i++)
            {
                var charm = pool[i];

                var price = (uint)Mathf.FloorToInt(charm.DefaultValue);
                charm.SetPrice(price);

                bundles.Add(new ShopItemBundle(charm));
            }

            return bundles;
        }

        #endregion
    }
}