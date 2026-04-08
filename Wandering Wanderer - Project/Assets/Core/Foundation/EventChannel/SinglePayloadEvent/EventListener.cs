using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Foundation.Events.SinglePayloadEvent {
    public abstract class EventListener<T> : MonoBehaviour
    {
        [Header("Subscribe Event SO")]
        [SerializeField] private EventChannelSO<T> eventChannelSo;
        [SerializeField] private UnityEvent<T> unityEvent;

        private void OnEnable()
        {
            if (eventChannelSo == null)
                return;
            eventChannelSo.AddListener(this);
        }

        void OnDisable()
        {
            if (eventChannelSo == null)
                return;
            eventChannelSo.RemoveListener(this); 
        }

        public void Raise(T value)
        {
            unityEvent?.Invoke(value);
        }
    }
}