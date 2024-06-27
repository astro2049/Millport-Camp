using UnityEngine;

namespace Abilities.Observer
{
    public abstract class ObserverComponent : MonoBehaviour, IObserver
    {
        public abstract bool OnNotify(MCEvent mcEvent);
    }
}
