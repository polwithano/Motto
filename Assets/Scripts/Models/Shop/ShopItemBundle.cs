using System;
using Events.Core;
using Events.Shop;
using Interfaces;
using UnityEngine;

namespace Models.Shop
{
    [Serializable]
    public class ShopItemBundle
    {
        [SerializeReference] public IBuyable Item;
        [field: SerializeField] public uint Price { get; private set; }

        public ShopItemBundle(IBuyable item)
        {
            Item = item;
            Price = item.DefaultValue; 
        }

        public bool CanBuy()
        {
            // return GameManager.Instance.Run.Money >= Price;
            return true; 
        }

        public bool TryPurchase()
        {
            if (!CanBuy())
                return false;

            // GameManager.Instance.Run.SpendMoney(Price);
            // Item.ProcessPurchase();
            
            Bus<PurchaseProcessedEvent>.Raise(new PurchaseProcessedEvent(this));
                
            return true;
        }
    }
}