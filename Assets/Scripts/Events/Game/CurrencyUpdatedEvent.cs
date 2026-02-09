using Events.Core;

namespace Events.Game
{
    public struct CurrencyUpdatedEvent : IEvent
    {
        public uint Currency { get; private set; }
        
        public CurrencyUpdatedEvent(uint currency)
        {
            Currency = currency;
        }
    }
}