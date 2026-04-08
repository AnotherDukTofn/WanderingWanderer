using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Foundation.Events.SinglePayloadEvent {
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        private bool _isRunning;
        private readonly List<EventListener<T>> _listeners = new List<EventListener<T>>();
        private readonly List<Action<T>> _actionListeners = new List<Action<T>>();

        public void RaiseEvent(T value)
        {
            if (_isRunning)
                return;
            try
            {
                if(_listeners == null)
                    return;
                _isRunning = true;
                List<EventListener<T>> snapshot = new List<EventListener<T>>(_listeners);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    snapshot[i]?.Raise(value);
                }
                var actionSnapshot = new List<Action<T>>(_actionListeners);
                for (int i = 0; i < actionSnapshot.Count; i++)
                {
                    actionSnapshot[i]?.Invoke(value);
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void AddListener(EventListener<T> listener)
        {
            if (listener == null)
                return;
            _listeners.Add(listener);
        }

        public void RemoveListener(EventListener<T> listener)
        {
            if (listener == null)
                return;
            _listeners.Remove(listener);
        }

        public void AddListener(Action<T> action)
        {
            if (action == null) return;
            _actionListeners.Add(action);
        }

        public void RemoveListener(Action<T> action)
        {
            if (action == null) return;
            _actionListeners.Remove(action);
        }
        
    }
}