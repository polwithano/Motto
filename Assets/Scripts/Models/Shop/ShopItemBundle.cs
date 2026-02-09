using System;
using Interfaces;
using Managers;
using UnityEngine;

namespace Models.Shop
{
    [Serializable]
    public class ShopItemBundle
    {
        [SerializeReference] public IBuyable Item;
        [field: SerializeField] public int Price { get; private set; }

        public ShopItemBundle(IBuyable item, int price)
        {
            Item = item;
            Price = price;
        }

        public bool CanBuy()
        {
            // return GameManager.Instance.Run.Money >= Price;
            return true; 
        }

        public bool Purchase()
        {
            if (!CanBuy())
                return false;

            // GameManager.Instance.Run.SpendMoney(Price);
            
            Item.ProcessPurchase();
            return true;
        }
    }
}