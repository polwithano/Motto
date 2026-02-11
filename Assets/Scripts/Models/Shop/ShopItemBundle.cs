using System;
using Events.Core;
using Events.Game;
using Events.Shop;
using Interfaces;
using Managers;
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
            return GameManager.Instance.Run.Currency >= Price;
        }

        public bool TryPurchase()
        {
            if (!CanBuy()) return false;
            if (!GameManager.Instance.Run.TryPurchase(Price)) return false;
            
            Bus<CurrencyUpdatedEvent>.Raise(new CurrencyUpdatedEvent(
                CurrencyType.Default,
                GameManager.Instance.Run.Currency));
            
            Bus<PurchaseProcessedEvent>.Raise(new PurchaseProcessedEvent(this));

            return true;
        }
    }
}