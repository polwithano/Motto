using System.Collections.Generic;
using Interfaces;
using Models;
using UnityEngine;

namespace Managers
{
    [System.Serializable]
    public class ShopItemBundle
    {
        [SerializeReference] public IBuyable Item; 
        [field: SerializeField] public int Price     { get; private set; }
        
        public ShopItemBundle(IBuyable item, int price) => (Item, Price) = (item, price);

        public bool ForcePurchase()
        {
            Item.ProcessPurchase();
            return true; 
        }
    }
    
    public class ShopManager : MonoBehaviourSingleton<ShopManager>
    {
        [SerializeField] private List<TileModifierSO> tileModifiersPool;
        [SerializeField] private int defaultRerollPrice = 10; 
        
        [field: SerializeField] public List<Tile> Tiles                 { get; private set; } = new();
        [field: SerializeField] public List<ShopItemBundle> ShopTiles   { get; private set; } = new();
        [field: SerializeField] public int RerollPrice  { get; private set; }

        public void InitializeShop()
        {
            RerollPrice = defaultRerollPrice;
            
            ClearShopItems();
            InstantiateShopItems();
        }

        public void RerollShop()
        {
            RerollPrice += defaultRerollPrice;
            
            ClearShopItems();
            InstantiateShopItems();
        }
        
        private void InstantiateShopItems()
        {
            var characters = GameManager.Instance.TileDistributionRule.CharacterRules; 
            
            for (var i = 0; i < 4; i++)
            {
                var rule = characters[Random.Range(0, characters.Count)];
                var modifier = tileModifiersPool[Random.Range(0, tileModifiersPool.Count)];
                var tile = new Tile(rule.character, rule.pointValue, rule.isBlank, modifier);

                var price = Mathf.FloorToInt(tile.Points * modifier.PriceModifier); 
                var bundle = new ShopItemBundle(tile, price);
                
                ShopTiles.Add(bundle);
                Tiles.Add(tile);
            }
        }

        private void ClearShopItems()
        {
            ShopTiles.Clear();
            Tiles.Clear();
        }
    }
}