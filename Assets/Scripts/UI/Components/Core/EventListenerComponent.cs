using Events.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Components.Core
{
    public abstract class EventListenerComponent<TEvent> 
        : MonoBehaviour
        where TEvent : IEvent
    {
        public UnityEvent OnEventSucceeded; 
        public UnityEvent OnEventFailed;

        protected virtual void OnEnable()
        
        {
            Bus<TEvent>.OnEvent += HandleEventInternal; 
        }

        protected virtual void OnDisable()
        {
            Bus<TEvent>.OnEvent -= HandleEventInternal; 
        }

        private void HandleEventInternal(TEvent evt)
        {
            if (HandleEvent(evt)) 
                OnEventSucceeded.Invoke();
            else
                OnEventFailed.Invoke();
        }
        
        /// <summary>
        /// Return true if the event was successfully processed
        /// </summary>
        protected abstract bool HandleEvent(TEvent evt);
    }
}