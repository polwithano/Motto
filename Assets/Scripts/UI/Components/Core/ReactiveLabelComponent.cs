using Events.Core;
using TMPro;
using UnityEngine;

namespace UI.Components.Core
{
    public abstract class ReactiveLabelComponent<TEvent, TValue> 
        : EventListenerComponent<TEvent>
        where TEvent : IEvent
    {
        [SerializeField] protected TextMeshProUGUI label;
        [SerializeField] protected string format = "{0}";

        protected override bool HandleEvent(TEvent evt)
        {
            if (!ShouldHandle(evt)) 
                return false;
            
            label.text = string.Format(format, ExtractValue(evt));
            
            return true; 
        }
        
        protected virtual bool ShouldHandle(TEvent evt) => true;
        protected abstract TValue ExtractValue(TEvent evt); 
    }
}