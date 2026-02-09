using Events.Core;
using Interfaces;
using Models.Shop;

namespace Events.Shop
{
    public struct PurchaseProcessedEvent : IEvent
    {
        public ShopItemBundle ItemBundle { get; private set; } 
        public IBuyable Buyable {get; private set;}

        public PurchaseProcessedEvent(ShopItemBundle bundle)
        {
            ItemBundle = bundle;
            Buyable = bundle.Item; 
        }
    }
}