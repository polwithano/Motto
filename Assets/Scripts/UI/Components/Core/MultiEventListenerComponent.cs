using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Events.Core;

namespace UI.Components.Core
{
    public abstract class MultiEventListenerComponent : MonoBehaviour
    {
        public UnityEvent OnEventSucceeded;
        public UnityEvent OnEventFailed;

        private readonly List<Action> _unsubscribeActions = new();

        protected virtual void OnEnable()
        {
            RegisterEvents();
        }

        protected virtual void OnDisable()
        {
            foreach (var unsubscribe in _unsubscribeActions)
                unsubscribe.Invoke();

            _unsubscribeActions.Clear();
        }

        protected void ListenTo<TEvent>() where TEvent : IEvent
        {
            void Handler(TEvent evt) => OnAnyEvent(evt);

            Bus<TEvent>.OnEvent += Handler;
            _unsubscribeActions.Add(() => Bus<TEvent>.OnEvent -= Handler);
        }

        protected abstract void RegisterEvents();
        protected abstract void OnAnyEvent(IEvent evt);

        protected void InvokeSuccess() => OnEventSucceeded?.Invoke();
        protected void InvokeFailed() => OnEventFailed?.Invoke();
    }
}