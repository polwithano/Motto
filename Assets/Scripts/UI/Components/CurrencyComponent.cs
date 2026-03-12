using Events.Game;
using Models;
using UI.Components.Core;
using UnityEngine;

namespace UI.Components
{
    public class CurrencyComponent : ReactiveLabelComponent<CurrencyUpdatedEvent, uint>
    {
        [SerializeField] private CurrencyType currencyType;
        
        protected override bool ShouldHandle(CurrencyUpdatedEvent evt)
        {
            return evt.Currency == currencyType;
        }

        protected override uint ExtractValue(CurrencyUpdatedEvent evt)
        {
            return evt.Amount;
        }

        protected override bool HandleEvent(CurrencyUpdatedEvent evt)
        {
            if (!ShouldHandle(evt))
                return false;

            label.text = evt.Amount.ToString();
            return true;
        }
    }
}
