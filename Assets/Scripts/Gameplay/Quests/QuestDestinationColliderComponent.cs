using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Quests
{
    public class QuestDestinationColliderComponent : MonoBehaviour
    {
        [HideInInspector] public UnityEvent reachedByPlayer = new UnityEvent();

        private void OnTriggerEnter(Collider other)
        {
            reachedByPlayer.Invoke();
        }
    }
}
