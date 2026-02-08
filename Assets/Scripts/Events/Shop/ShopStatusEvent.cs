using Events.Core;

namespace Events.Shop
{
    public enum ShopStatus
    {
        Open, 
        Closed,
        Reroll
    }
    
    public struct ShopStatusEvent : IEvent
    {
        public ShopStatus Status {get ; private set;}

        public ShopStatusEvent(ShopStatus status)
        {
            Status = status;
        }
    }
}