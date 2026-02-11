using System;
using Events.Core;
using Events.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Components
{
    public class CurrencyComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] public CurrencyType currencyType;
        
        public UnityEvent OnEventSucceeded; 
        
        #region Monobehaviour
        private void OnEnable()
        {
            Bus<CurrencyUpdatedEvent>.OnEvent += HandleCurrencyUpdated; 
        }

        private void OnDisable()
        {
            Bus<CurrencyUpdatedEvent>.OnEvent -= HandleCurrencyUpdated; 
        }
        #endregion
        
        private void HandleCurrencyUpdated(CurrencyUpdatedEvent evt)
        {
            if (evt.Currency != currencyType) return;
            
            text.text = GetCurrencySymbol(evt.Currency) + evt.Amount;
            OnEventSucceeded?.Invoke();
        }

        private string GetCurrencySymbol(CurrencyType currency)
        {
            return currency switch
            {
                CurrencyType.Default => "$",
                CurrencyType.Special => "@",
                _ => throw new ArgumentOutOfRangeException(nameof(currency), currency, null)
            };
        }
    }
}
