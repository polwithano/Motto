using System;
using Events.Core;
using Events.Game;
using Models;
using TMPro;
using UI.Components.Core;
using UnityEngine;
using UnityEngine.Events;

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

            label.text = GetCurrencySymbol(evt.Currency) + evt.Amount;
            return true;
        }
        
        private string GetCurrencySymbol(CurrencyType currency)
        {
            return currency switch
            {
                CurrencyType.Soft => "$",
                CurrencyType.Hard => "@",
                _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
            };
        }
    }
}
