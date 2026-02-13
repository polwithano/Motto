using Events.Core;
using Models;

namespace Events.Game
{
    public struct CurrencyUpdatedEvent : IEvent
    {
        public CurrencyType Currency { get; private set; }
        public uint Amount { get; private set; }
        
        public CurrencyUpdatedEvent(CurrencyType currency, uint amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }
}