using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivationDistanceColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Activate NPC
            other.transform.parent.GetComponent<NPCActivatorComponent>().Activate();
        }

        private void OnTriggerExit(Collider other)
        {
            // Deactivate NPC
            other.transform.parent.GetComponent<NPCActivatorComponent>().Deactivate();
        }
    }
}
