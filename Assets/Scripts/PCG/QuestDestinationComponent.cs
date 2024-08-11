using UnityEngine;
using UnityEngine.Events;

namespace PCG
{
    public class QuestDestinationComponent : MonoBehaviour
    {
        [HideInInspector] public UnityEvent reachedByPlayer = new UnityEvent();

        private void OnTriggerEnter(Collider other)
        {
            reachedByPlayer.Invoke();
        }
    }
}
