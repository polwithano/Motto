using Events.Core;

namespace Events.Shop
{
    public enum ShopState
    {
        Opened, 
        Closed,
    }
    
    public struct ShopStateEvent : IEvent
    {
        public ShopState State {get ; private set;}

        public ShopStateEvent(ShopState state)
        {
            State = state;
        }
    }
}