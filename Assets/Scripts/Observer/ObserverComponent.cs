using UnityEngine;

namespace Observer
{
    public abstract class ObserverComponent : MonoBehaviour, IObserver
    {
        public abstract bool OnNotify(MCEvent mcEvent);
    }
}
