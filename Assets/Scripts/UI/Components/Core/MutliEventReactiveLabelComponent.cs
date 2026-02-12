using TMPro;
using UnityEngine;
using Events.Core;

namespace UI.Components.Core
{
    public abstract class MultiEventReactiveLabelComponent<TValue> 
        : MultiEventListenerComponent
    {
        [SerializeField] protected TextMeshProUGUI text;
        [SerializeField] private string format = "{0}";

        protected override void OnAnyEvent(IEvent evt)
        {
            if (!ShouldHandle(evt))
            {
                InvokeFailed();
                return;
            }

            var value = GetValue();
            text.text = string.Format(format, value);
            InvokeSuccess();
        }

        /// <summary>
        /// Allow event filtering if needed
        /// </summary>
        protected virtual bool ShouldHandle(IEvent evt) => true;

        /// <summary>
        /// How we display the value needed
        /// </summary>
        protected abstract TValue GetValue();
    }
}