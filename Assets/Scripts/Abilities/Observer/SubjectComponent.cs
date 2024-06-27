using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.Observer
{
    public class SubjectComponent : MonoBehaviour
    {
        private readonly Dictionary<EventType, HashSet<IObserver>> observers = new Dictionary<EventType, HashSet<IObserver>>();

        public SubjectComponent()
        {
            foreach (EventType eventType in Enum.GetValues(typeof(EventType))) {
                observers.Add(eventType, new HashSet<IObserver>());
            }
        }

        // Quick add
        public void AddObserver(IObserver observer)
        {
            foreach (KeyValuePair<EventType, HashSet<IObserver>> kv in observers) {
                kv.Value.Add(observer);
            }
        }

        public void AddObserver(IObserver observer, params EventType[] eventTypes)
        {
            foreach (EventType eventType in eventTypes) {
                observers[eventType].Add(observer);
            }
        }

        // Quick removal
        public void RemoveObserver(IObserver observer)
        {
            foreach (KeyValuePair<EventType, HashSet<IObserver>> kv in observers) {
                kv.Value.Remove(observer);
            }
        }

        public void RemoveObserver(IObserver observer, params EventType[] eventTypes)
        {
            foreach (EventType eventType in eventTypes) {
                observers[eventType].Remove(observer);
            }
        }

        public void NotifyObservers(MCEvent mcEvent)
        {
            foreach (IObserver observer in observers[mcEvent.type]) {
                observer.OnNotify(mcEvent);
            }
        }
    }
}
