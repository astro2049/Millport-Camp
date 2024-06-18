using System.Collections.Generic;
using UnityEngine;

namespace Observer
{
    public class SubjectComponent : MonoBehaviour
    {
        private List<IObserver> observers = new List<IObserver>();

        public void AddObserver(IObserver observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers(EventType mcEvent)
        {
            foreach (IObserver observer in observers) {
                observer.OnNotify(mcEvent);
            }
        }
    }
}
