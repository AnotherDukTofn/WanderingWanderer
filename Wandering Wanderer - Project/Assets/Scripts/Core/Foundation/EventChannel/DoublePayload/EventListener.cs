using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Foundation.Events.TwoPayloadEvent {
    public abstract class EventListener<T1, T2> : MonoBehaviour
    {
        [Header("Subscribe Event SO")]
        [SerializeField] EventChannelSO<T1, T2> eventChannelSo;
        [SerializeField] UnityEvent<T1, T2> unityEvent;

        private void OnEnable()
        {
            if (eventChannelSo == null)
            {
                return;
            }
            eventChannelSo.AddListener(this);
        }

        private void OnDisable()
        {
            if (eventChannelSo == null)
                return;
            eventChannelSo.RemoveListener(this);
        }

        public void Raise(T1 value1, T2 value2)
        {
            unityEvent?.Invoke(value1, value2);
        }
    }
}
