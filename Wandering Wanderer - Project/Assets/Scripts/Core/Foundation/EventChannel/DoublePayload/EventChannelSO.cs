using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Foundation.Events.TwoPayloadEvent {
    public abstract class EventChannelSO<T1, T2> : ScriptableObject
    {
        // ----- Retrancy Safe -----
        private bool _isRunning;
        
        // ----- Event Listeners -----
        private readonly List<EventListener<T1, T2>> _listeners = new List<EventListener<T1, T2>>();
        private readonly List<Action<T1, T2>> _actionListeners = new List<Action<T1, T2>>();

        public void RaiseEvent(T1 value1, T2 value2)
        {
            if (_isRunning)
                return;
            try
            {
                _isRunning = true;
                var listenerSnapshot = new List<EventListener<T1, T2>>(_listeners);
                for (int i = 0; i < listenerSnapshot.Count; i++)
                    listenerSnapshot[i]?.Raise(value1, value2);

                var actionSnapshot = new List<Action<T1, T2>>(_actionListeners);
                for (int i = 0; i < actionSnapshot.Count; i++)
                    actionSnapshot[i]?.Invoke(value1, value2);
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void AddListener(EventListener<T1, T2> listener)
        {
            if (listener == null) return;
            _listeners.Add(listener);
        }

        public void RemoveListener(EventListener<T1, T2> listener)
        {
            if (listener == null) return;
            _listeners.Remove(listener);
        }

        public void AddListener(Action<T1, T2> action)
        {
            if (action == null) return;
            _actionListeners.Add(action);
        }

        public void RemoveListener(Action<T1, T2> action)
        {
            if (action == null) return;
            _actionListeners.Remove(action);
        }
    }
}
