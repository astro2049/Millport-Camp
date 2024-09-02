using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActorActivatorColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Activate NPC
            other.transform.GetComponent<ActivateeSwitchComponent>().Activate();
        }

        private void OnTriggerExit(Collider other)
        {
            // Deactivate NPC
            other.transform.GetComponent<ActivateeSwitchComponent>().Deactivate();
        }
    }
}
